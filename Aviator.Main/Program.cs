using Aviator.Acars;
using Aviator.Acars.Config;
using Aviator.Network.Input;
using Aviator.Network.Output;

var builder = WebApplication.CreateBuilder(args);

var acarsConfig = builder.Configuration.GetSection(AcarsConfig.Section).Get<AcarsConfig>();

if (acarsConfig == null)
{
    throw new InvalidOperationException($"Missing {AcarsConfig.Section} config");
}

// Network
builder.Services.AddSingleton<InputBuilder>();
builder.Services.AddSingleton<OutputBuilder>();

// Acars
builder.Services.AddSingleton(acarsConfig);
builder.Services.AddSingleton<AcarsIoManager>();
builder.Services.AddHostedService<AcarsService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();
