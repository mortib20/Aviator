using Aviator.Library.Acars;
using Aviator.Library.Acars.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Aviator.DependencyInjection;

public static class AviatorExtension
{
    public static WebApplicationBuilder AddAviator(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("aviator.json");
        
        // AcarsRouterSettings Section
        var acarsRouterSettings = builder.Configuration.GetSection(AcarsRouterSettings.SectionName);
        builder.Services.Configure<AcarsRouterSettings>(acarsRouterSettings);

        // Add AcarsRouter
        builder.Services.AddAcarsRouter();
        
        return builder;
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

    public static WebApplication MapAviatorStatus(this WebApplication app)
    {
        app.MapGet("/status", ([FromServices] AcarsOutputManager outputManager) =>
        {
            return outputManager.Outputs
                .ToDictionary(keyValuePair => keyValuePair.Key, valuePair => valuePair.Value.Select(output => new
                {
                    EndPoint = new
                    {
                        output.EndPoint.Host,
                        output.EndPoint.Port,
                        Protocol = output.EndPoint.Protocol.ToString()
                    },
                    State = output.State.ToString()
                }).ToList());
        });

        return app;
    }
}