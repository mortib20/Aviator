using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Aviator.Library.Acars;
using Aviator.Library.Acars.Settings;
using Aviator.Library.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aviator.DependencyInjection;

public static class AviatorExtension
{
    public static WebApplicationBuilder AddAviator(this WebApplicationBuilder builder)
    {
        // AcarsRouterSettings Section
        var acarsRouterSettings = builder.Configuration.GetSection(AcarsRouterSettings.SectionName);
        builder.Services.Configure<AcarsRouterSettings>(acarsRouterSettings);

        // Add AcarsRouter
        builder.Services.AddAcarsRouter();
        
        return builder;
    }

    public static WebApplication AddAviator(this WebApplication app)
    {

        app.MapGet("/status", ([FromServices] AcarsOutputManager outputManager) =>
        {
            return outputManager.Outputs
                .ToDictionary(keyValuePair => keyValuePair.Key, valuePair => valuePair.Value.Select(output => new
                {
                    EndPoint = new
                    {
                        Host = output.EndPoint.Host,
                        Port = output.EndPoint.Port,
                        Protocol = output.EndPoint.Protocol.ToString()
                    },
                    State = output.State.ToString()
                }).ToList());;
        });
        
        return app;
    }
}