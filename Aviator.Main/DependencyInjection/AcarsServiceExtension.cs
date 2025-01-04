using System.Collections.ObjectModel;
using Aviator.Acars;
using Aviator.Acars.Config;
using Aviator.Acars.Database;
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

        #region Metrics
        builder.Services.AddSingleton<IAcarsMetrics>(s =>
        {
            var metrics = new Collection<IAcarsMetrics>();
            
            if (acarsConfig.InfluxDb is not null && acarsConfig.InfluxDb!.Enabled)
            {
                metrics.Add(new InfluxDbMetrics(acarsConfig.InfluxDb, s.GetRequiredService<ILogger<InfluxDbMetrics>>()));
            }
            
            
            s.GetRequiredService<ILogger<AcarsService>>().LogInformation("Enabled Metric: {Types}", string.Join(", ", metrics.Select(acarsMetrics => acarsMetrics.GetType()).ToList()));
            
            return new AcarsMetrics(metrics);
        });
        #endregion
        
        #region Database

        builder.Services.AddSingleton<IAcarsDatabase>(s =>
        {
            var databases = new Collection<IAcarsDatabase>();

            if (acarsConfig.MongoDb is not null && acarsConfig.MongoDb!.Enabled)
            {
                databases.Add(new AcarsMongoDatabase(acarsConfig.MongoDb));
            }
            
            s.GetRequiredService<ILogger<AcarsService>>().LogInformation("Enabled Databases: {Types}", string.Join(", ", databases.Select(acarsMetrics => acarsMetrics.GetType()).ToList()));

            return new AcarsDatabase(databases);
        });
        #endregion

        #region HostedService
        builder.Services.AddHostedService<AcarsService>(s => SetupAcarsService(s, acarsConfig));
        #endregion

        return builder;
    }

    private static AcarsService SetupAcarsService(IServiceProvider s, AcarsConfig acarsConfig)
    {
        ArgumentNullException.ThrowIfNull(acarsConfig.Input);
        acarsConfig.Outputs ??= [];

        var input = s.GetRequiredService<InputBuilder>()
            .Create(acarsConfig.Input.Protocol, acarsConfig.Input.Host, acarsConfig.Input.Port);

        var outputBuilder = s.GetRequiredService<OutputBuilder>();
        var outputs = acarsConfig.Outputs.Select(selector: a => (a.Types, outputBuilder.Create(a.Protocol, a.Host, a.Port))).ToList();

        var types = Enum
            .GetValuesAsUnderlyingType<AcarsType>()
            .Cast<AcarsType>().ToList();
        
        var outputDictionary = types
            .ToDictionary<AcarsType, AcarsType, List<IOutput>>(k => k, v =>
            {
                return outputs.Where(b => b.Types.Contains(v)).Select(o => o.Item2).ToList();
            });

        var logger = s.GetRequiredService<ILogger<AcarsService>>();
        
        foreach (var type in types)
        {
            logger.LogInformation("Sending {Types} to: {Outputs}", type, string.Join(", ", outputDictionary[type].Select(f => f.EndPoint).ToList()));
        }

        var acarsIoManager = new AcarsIoManager(s.GetRequiredService<ILogger<AcarsIoManager>>(), input, outputDictionary);

        return new AcarsService(logger, acarsIoManager, s.GetRequiredService<IAcarsMetrics>(),
            s.GetRequiredService<IAcarsDatabase>());
    }
}