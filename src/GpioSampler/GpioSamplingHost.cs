namespace GpioSampler;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GpioSampler.Configuration;
using GpioSampler.Sampling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plotly.NET;

internal sealed class GpioSamplingHost : BackgroundService
{
    private readonly IGpioSamplingService _gpioSamplingService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<GpioSamplingHost> _logger;
    private readonly IOptions<SamplerConfiguration> _options;

    public GpioSamplingHost(
        IGpioSamplingService gpioSamplingService,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptions<SamplerConfiguration> options,
        ILogger<GpioSamplingHost> logger)
    {
        ArgumentNullException.ThrowIfNull(gpioSamplingService);
        ArgumentNullException.ThrowIfNull(hostApplicationLifetime);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _gpioSamplingService = gpioSamplingService;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _options = options;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        return RunAsync(stoppingToken);
    }

    private async Task RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sampler = _gpioSamplingService.SampleAsync(
            stopwatch: Stopwatch.StartNew(),
            lines: _options.Value.Lines.ToImmutableArray(),
            interval: TimeSpan.Parse(_options.Value.Interval),
            duration: TimeSpan.Parse(_options.Value.Duration),
            cancellationToken: cancellationToken);

        var samples = new List<GpioLineChange>();

        await foreach (var sample in sampler)
        {
            samples.Add(sample);
        }

        var charts = samples
            .GroupBy(sample => sample.Label)
            .Select((group, index) => (Index: index, Group: group))
            .Select(group => Chart2D.Chart.Line<double, int, string>(
                x: group.Group.Select(sample => sample.RelativeOffset.TotalMilliseconds),
                y: group.Group.Select(sample => (sample.Value ? 1 : 0) + (group.Index * 2)),
                Name: group.Group.Key,
                Line: Line.init(Shape: StyleParam.Shape.Hv))
                .WithLayout(Layout.init<string>(Width: 1200)))
            .ToImmutableArray();

        Chart.Combine(charts).SaveHtml("sample.html");

        _logger.LogInformation("Chart exported to sample.html");
        _hostApplicationLifetime.StopApplication();
    }
}
