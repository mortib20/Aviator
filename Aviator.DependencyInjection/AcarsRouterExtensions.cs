using Aviator.Library.Acars;
using Aviator.Library.Acars.Config;
using Aviator.Library.IO.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aviator.DependencyInjection;

public static class AcarsRouterExtensions
{
    public static IServiceCollection AddAcarsRouter(this IServiceCollection services)
    {
        // AcarsRouterInput
        services.AddSingleton<IInput>(s =>
        {
            var settings = s.GetService<IOptions<AcarsRouterConfig>>()?.Value;

            return settings?.InputProtocol switch
            {
                InputProtocol.Tcp => new TcpInput(s.GetRequiredService<ILogger<TcpInput>>()),
                InputProtocol.Udp => new UdpInput(s.GetRequiredService<ILogger<UdpInput>>()),
                _ => throw new ArgumentOutOfRangeException(settings?.InputProtocol.ToString())
            };
        });

        services.AddSingleton<AcarsOutputManager>();

        // AcarsRouterWorker
        services.AddHostedService<AcarsRouterWorker>();
        
        return services;
    }
}