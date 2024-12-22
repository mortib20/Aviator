using System.Text.Json;
using Aviator.Main.DependencyInjection;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.AddNetworkUtilities();
builder.AddAcarsService();

builder.Logging.AddSimpleConsole(s =>
{
    s.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    s.ColorBehavior = LoggerColorBehavior.Enabled;
});

var app = builder.Build();

app.MapGet("/", () => JsonSerializer.Serialize("Hello World!"));

await app.RunAsync();