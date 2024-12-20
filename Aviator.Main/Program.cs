using Aviator.Acars;
using Aviator.Acars.Config;
using Aviator.Acars.Metrics;
using Aviator.Network.Input;
using Aviator.Network.Output;

var builder = WebApplication.CreateBuilder(args);

var acarsConfig = builder.Configuration.GetSection(AcarsConfig.Section).Get<AcarsConfig>();

ArgumentNullException.ThrowIfNull(acarsConfig);

// Network
builder.Services.AddSingleton<InputBuilder>();
builder.Services.AddSingleton<OutputBuilder>();

// Acars
builder.Services.AddSingleton(acarsConfig);

var acarsMetricsList = new List<IAcarsMetrics>();

if (acarsConfig.InfluxDb is not null && acarsConfig.InfluxDb!.Enabled)
{
    acarsMetricsList.Add(new InfluxDbMetrics(acarsConfig.InfluxDb));
}

builder.Services.AddSingleton<IAcarsMetrics>(s => new AcarsMetrics(acarsMetricsList));

builder.Services.AddSingleton<AcarsIoManager>(s =>
{
    ArgumentNullException.ThrowIfNull(acarsConfig.Input);
    ArgumentNullException.ThrowIfNull(acarsConfig.Outputs);
    
    var input = s.GetRequiredService<InputBuilder>().Create(acarsConfig.Input.Protocol, acarsConfig.Input.Host, acarsConfig.Input.Port);
    
    
    var outputsConfig = acarsConfig.Outputs;
    var types = outputsConfig.Select(x => x.Type).Distinct().ToList();
    var outputs = types.ToDictionary(type => type, type => outputsConfig.Where(x => x.Type == type)
        .Select(x => s.GetRequiredService<OutputBuilder>().Create(x.Protocol, x.Host, x.Port))
        .ToList());
    
    return new AcarsIoManager(s.GetRequiredService<ILogger<AcarsIoManager>>(), input, outputs);
});
builder.Services.AddHostedService<AcarsService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();
