namespace GpioSampler.Sampling;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GpioSampler.Configuration;

internal sealed class GpioSamplingService : IGpioSamplingService
{
    public async IAsyncEnumerable<GpioLineChange> SampleAsync(
        Stopwatch stopwatch,
        ImmutableArray<LineConfiguration> lines,
        TimeSpan interval,
        TimeSpan duration,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var periodicTimer = new PeriodicTimer(interval);

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationTokenSource.CancelAfter(duration);

        cancellationTokenSource.Token.UnsafeRegister(
            callback: state => Unsafe.As<object?, PeriodicTimer>(ref state).Dispose(),
            state: periodicTimer);

        var stateStore = GC.AllocateUninitializedArray<int>(lines.Length);
        stateStore.AsSpan().Fill(byte.MaxValue);

        while (await periodicTimer.WaitForNextTickAsync(CancellationToken.None).ConfigureAwait(false))
        {
            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];

                var fileStream = new FileStream(
                    path: line.FilePath,
                    mode: FileMode.Open,
                    access: FileAccess.Read,
                    share: FileShare.ReadWrite,
                    bufferSize: 8,
                    options: FileOptions.SequentialScan);

                await using var _ = fileStream.ConfigureAwait(false);

                var value = fileStream.ReadByte() is not (byte)'0';
                var numericValue = value ? 1 : 0;

                if (stateStore[index] == numericValue)
                {
                    continue;
                }

                stateStore[index] = numericValue;

                yield return new GpioLineChange(
                    Label: line.Label,
                    Value: value,
                    RelativeOffset: stopwatch.Elapsed);
            }
        }

        for (var index = 0; index < lines.Length; index++)
        {
            yield return new GpioLineChange(
                Label: lines[index].Label,
                Value: stateStore[index] is not 0,
                RelativeOffset: stopwatch.Elapsed);
        }
    }
}
