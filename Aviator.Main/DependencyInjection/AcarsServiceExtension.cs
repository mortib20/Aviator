using System.Collections.ObjectModel;
using Aviator.Acars;
using Aviator.Acars.Config;
using Aviator.Acars.Database;
using Aviator.Acars.Entities;
using Aviator.Acars.Metrics;
using Aviator.Network.Input;
using Aviator.Network.Output;

namespace Aviator.Main.DependencyInjection;

public static class AcarsServiceExtension
{
    public static WebApplicationBuilder AddAcarsService(this WebApplicationBuilder builder)
    {
        var acarsConfig = builder.Configuration.GetSection(AcarsConfig.Section).Get<AcarsConfig>();
        ArgumentNullException.ThrowIfNull(acarsConfig);

        SetupMetrics(builder, acarsConfig);
        SetupDatabases(builder, acarsConfig);
        builder.Services.AddHostedService<AcarsService>(s => SetupAcarsService(s, acarsConfig));

        return builder;
    }

    private static void SetupMetrics(WebApplicationBuilder builder, AcarsConfig acarsConfig)
    {
        builder.Services.AddSingleton<IAcarsMetrics>(s =>
        {
            var logger = s.GetRequiredService<ILogger<AcarsService>>();
            var metrics = new Collection<IAcarsMetrics>();
            
            if (acarsConfig.InfluxDb is not null && acarsConfig.InfluxDb!.Enabled)
            {
                var metricLogger = s.GetRequiredService<ILogger<InfluxDbMetrics>>();
                var metric = new InfluxDbMetrics(acarsConfig.InfluxDb, metricLogger);
                metrics.Add(metric);
            }
            
            var enabledMetrics = metrics.Select(acarsMetrics => acarsMetrics.GetType()).ToList();
            logger.LogInformation("Enabled Metric: {Types}", string.Join(", ", enabledMetrics));
            
            return new AcarsMetrics(metrics);
        });
    }

    private static void SetupDatabases(WebApplicationBuilder builder, AcarsConfig acarsConfig)
    {
        builder.Services.AddSingleton<IAcarsDatabase>(s =>
        {
            var logger = s.GetRequiredService<ILogger<AcarsService>>();
            var databases = new Collection<IAcarsDatabase>();

            if (acarsConfig.MongoDb is not null && acarsConfig.MongoDb!.Enabled)
            {
                var mongoDb = new AcarsMongoDatabase(acarsConfig.MongoDb);
                databases.Add(mongoDb);
            }

            var enabledDatabases = databases.Select(acarsMetrics => acarsMetrics.GetType()).ToList();
            logger.LogInformation("Enabled Databases: {Types}", string.Join(", ", enabledDatabases));

            return new AcarsDatabase(databases);
        });
    }

    private static AcarsService SetupAcarsService(IServiceProvider s, AcarsConfig acarsConfig)
    {
        ArgumentNullException.ThrowIfNull(acarsConfig.Input);

        var outputDictionary = CreateOutputDictionary(s, acarsConfig.Outputs);
        
        var input = s.GetRequiredService<InputBuilder>().Create(acarsConfig.Input.Protocol, acarsConfig.Input.Host, acarsConfig.Input.Port);
        var acarsIoManager = new AcarsIoManager(s.GetRequiredService<ILogger<AcarsIoManager>>(), input, outputDictionary);

        return new AcarsService(s.GetRequiredService<ILogger<AcarsService>>(), acarsIoManager, s.GetRequiredService<IAcarsMetrics>(),
            s.GetRequiredService<IAcarsDatabase>());
    }

    private static Dictionary<SourceType,List<IOutput>> CreateOutputDictionary(IServiceProvider s, List<OutputEndpointConfig> acarsConfig)
    {
        var outputBuilder = s.GetRequiredService<OutputBuilder>();
        var outputsTuple = acarsConfig
            .Select(selector: a => (a.Types, outputBuilder.Create(a.Protocol, a.Host, a.Port))).ToList();
        
        var frameTypes = Enum.GetValues<SourceType>().ToList();

        var outputDictionary = frameTypes
            .ToDictionary<SourceType, SourceType, List<IOutput>>(frameType => frameType, frameType => outputsTuple.Where(b => b.Types.Contains(frameType)).Select(o => o.Item2).ToList());
        
        var logger = s.GetRequiredService<ILogger<AcarsService>>();
        logger.LogInformation("{B}", string.Join(Environment.NewLine, frameTypes.Select(t => $"Sending {t} to: {string.Join(", ", outputDictionary[t].Select(f => f.EndPoint).ToList())}")));
        
        return outputDictionary;
    }
}