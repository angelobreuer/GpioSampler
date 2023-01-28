namespace GpioSampler.Sampling;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using GpioSampler.Configuration;

public interface IGpioSamplingService
{
    IAsyncEnumerable<GpioLineChange> SampleAsync(
        Stopwatch stopwatch,
        ImmutableArray<LineConfiguration> lines,
        TimeSpan interval,
        TimeSpan duration,
        CancellationToken cancellationToken = default);
}