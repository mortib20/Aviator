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
            
            
            s.GetRequiredService<ILogger<AcarsService>>().LogInformation("Enabled Metric: {Types}", string.Join(',', metrics.Select(acarsMetrics => acarsMetrics.GetType()).ToList()));
            
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
            
            s.GetRequiredService<ILogger<AcarsService>>().LogInformation("Enabled Databases: {Types}", string.Join(',', databases.Select(acarsMetrics => acarsMetrics.GetType()).ToList()));

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

        var outputsConfig = acarsConfig.Outputs;
        var types = outputsConfig.Select(x => x.Type).Distinct().ToList();
        var outputs = types.ToDictionary(type => type, type => outputsConfig.Where(x => x.Type == type)
            .Select(x => s.GetRequiredService<OutputBuilder>().Create(x.Protocol, x.Host, x.Port))
            .AsEnumerable());

        var logger = s.GetRequiredService<ILogger<AcarsService>>();
        var acarsIoManager = new AcarsIoManager(s.GetRequiredService<ILogger<AcarsIoManager>>(), input, outputs);

        return new AcarsService(logger, acarsIoManager, s.GetRequiredService<IAcarsMetrics>(),
            s.GetRequiredService<IAcarsDatabase>());
    }
}