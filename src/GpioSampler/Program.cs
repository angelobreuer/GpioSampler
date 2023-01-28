using GpioSampler;
using GpioSampler.Configuration;
using GpioSampler.Sampling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new HostApplicationBuilder(args);

builder.Services.Configure<SamplerConfiguration>(builder.Configuration.GetRequiredSection("Sampler"));
builder.Services.AddSingleton<IGpioSamplingService, GpioSamplingService>();
builder.Services.AddHostedService<GpioSamplingHost>();

var app = builder.Build();

app.Run();