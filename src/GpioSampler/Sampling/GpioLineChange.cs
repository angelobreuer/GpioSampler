namespace GpioSampler.Sampling;

using System;

public readonly record struct GpioLineChange(
    string Label,
    bool Value,
    TimeSpan RelativeOffset);