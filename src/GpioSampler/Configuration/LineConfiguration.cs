namespace GpioSampler.Configuration;
public sealed record class LineConfiguration
{
    public required string Label { get; set; }

    public required string FilePath { get; set; }
}
