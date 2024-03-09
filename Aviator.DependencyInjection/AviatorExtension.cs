using Aviator.Library.Acars.Settings;
using Microsoft.AspNetCore.Builder;
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
}