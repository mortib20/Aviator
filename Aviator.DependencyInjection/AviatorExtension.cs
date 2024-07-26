using Aviator.Library.Acars;
using Aviator.Library.Acars.Config;
using Aviator.Library.Database;
using Aviator.Library.Database.Config;
using Aviator.Library.Metrics;
using Aviator.Library.Metrics.InfluxDB;
using Aviator.Library.Metrics.Prometheus;
using CouchDB.Driver.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Aviator.DependencyInjection;

public static class AviatorExtension
{
    public static WebApplicationBuilder AddAviator(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("aviator.json");
        
        // AcarsRouterSettings Section
        var acarsRouterSettings = builder.Configuration.GetSection(AcarsRouterConfig.SectionName);
        builder.Services.Configure<AcarsRouterConfig>(acarsRouterSettings);

        // CouchDbConfig
        var couchDbConfig = builder.Configuration.GetSection(CouchDbConfig.SectionName).Get<CouchDbConfig>();

        builder.Services.AddCouchContext<CouchDbContext>(optionsBuilder => 
            optionsBuilder
                .UseEndpoint(couchDbConfig.Endpoint)
                .UseBasicAuthentication(username: couchDbConfig.Username, password: couchDbConfig.Password)
                .EnsureDatabaseExists());
        
        // Add Metrics
        AddMetrics(builder);
        
        // Add AcarsRouter
        builder.Services.AddAcarsRouter();
        
        return builder;
    }

    private static void AddMetrics(WebApplicationBuilder builder)
    {
        var metricsList = new List<IAviatorMetrics>()
        {
            new PrometheusMetrics()
        };
        
        // InfluxDBConfig Section
        var influxConfig = builder.Configuration
            .GetSection(InfluxDbMetricsConfig.SectionName)
            .Get<InfluxDbMetricsConfig>();
        
        // Add InfluxDB if found and enabled
        if (influxConfig is not null && influxConfig.Enabled)
        {
            metricsList.Add(new InfluxDbMetrics(influxConfig, builder.Services.BuildServiceProvider().GetRequiredService<ILogger<InfluxDbMetrics>>()));
        }
        
        // Add Metrics List
        builder.Services.AddSingleton(metricsList);
        
        // Add Metrics Writer
        builder.Services.AddSingleton<AviatorMetrics>();
    }

    public static WebApplication AddAviator(this WebApplication app)
    {
        app.UseCors(s =>
        {
            s.AllowAnyHeader();
            s.AllowAnyMethod();
            s.SetIsOriginAllowed(_ => true);
            s.AllowCredentials();
        });
        
        app.MapMetrics();
        app.MapAviatorStatus();
        app.MapHub<AcarsHub>("/hub/acars");
        
        return app;
    }

    private static WebApplication MapAviatorStatus(this WebApplication app)
    {
        app.MapGet("/status", ([FromServices] AcarsOutputManager outputManager) =>
        {
            return outputManager.Outputs
                .ToDictionary(keyValuePair => keyValuePair.Key, valuePair => valuePair.Value.Select(output => new OutputStatus
                    {
                        Enabled = output.Enabled,
                        State = output.State.ToString(),
                        LastErrorDateTime = output.LastError,
                        Endpoint = new OutputStatusEndpoint
                        {
                            Host = output.EndPoint.Host,
                            Port = output.EndPoint.Port,
                            Protocol = output.EndPoint.Protocol.ToString()
                        }
                    })
                    .ToList());
        });

        return app;
    }
}

internal class OutputStatus
{
    public bool Enabled { get; set; }
    public string? State { get; set; }
    public DateTime? LastErrorDateTime { get; set; }
    public OutputStatusEndpoint? Endpoint { get; set; }
}

internal class OutputStatusEndpoint
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public string Protocol { get; set; }
}