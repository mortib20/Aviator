using System.Text.Json;
using Aviator.Acars;
using Aviator.Acars.Config;
using Aviator.Acars.Metrics;
using Aviator.Network.Input;
using Aviator.Network.Output;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

var acarsConfig = builder.Configuration.GetSection(AcarsConfig.Section).Get<AcarsConfig>();

ArgumentNullException.ThrowIfNull(acarsConfig);

// Network
builder.Services.AddSingleton<InputBuilder>();
builder.Services.AddSingleton<OutputBuilder>();

// Acars
builder.Services.AddSingleton(acarsConfig);


if (acarsConfig.InfluxDb is not null && acarsConfig.InfluxDb!.Enabled)
{
    builder.Services.AddSingleton<InfluxDbMetrics>(s => new InfluxDbMetrics(acarsConfig.InfluxDb, s.GetRequiredService<ILogger<InfluxDbMetrics>>()));
}

builder.Services.AddSingleton<IAcarsMetrics>(s =>
{
    var acarsMetricsList = new List<IAcarsMetrics>()
    {
        s.GetRequiredService<InfluxDbMetrics>()
    }; 
    return new AcarsMetrics(acarsMetricsList);
});

builder.Services.AddSingleton<AcarsIoManager>(s =>
{
    ArgumentNullException.ThrowIfNull(acarsConfig.Input);
    acarsConfig.Outputs ??= [];

    var input = s.GetRequiredService<InputBuilder>()
        .Create(acarsConfig.Input.Protocol, acarsConfig.Input.Host, acarsConfig.Input.Port);


    var outputsConfig = acarsConfig.Outputs;
    var types = outputsConfig.Select(x => x.Type).Distinct().ToList();
    var outputs = types.ToDictionary(type => type, type => outputsConfig.Where(x => x.Type == type)
        .Select(x => s.GetRequiredService<OutputBuilder>().Create(x.Protocol, x.Host, x.Port))
        .ToList());

    return new AcarsIoManager(s.GetRequiredService<ILogger<AcarsIoManager>>(), input, outputs);
});
builder.Services.AddHostedService<AcarsService>();

builder.Logging.AddSimpleConsole(s =>
{
    s.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    s.ColorBehavior = LoggerColorBehavior.Enabled;
});

var app = builder.Build();

app.MapGet("/", () => JsonSerializer.Serialize("Hello World!"));

await app.RunAsync();