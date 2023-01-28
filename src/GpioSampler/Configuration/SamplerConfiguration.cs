namespace GpioSampler.Configuration;

public sealed record class SamplerConfiguration
{
    public string Duration { get; set; } = "00:00:05.000";

    public string Interval { get; set; } = "00:00:00.050";

    public required LineConfiguration[] Lines { get; set; }
}
