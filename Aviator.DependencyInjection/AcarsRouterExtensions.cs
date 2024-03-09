using Aviator.Library;
using Aviator.Library.Acars;
using Aviator.Library.Acars.Settings;
using Aviator.Library.IO;
using Aviator.Library.IO.Input;
using Microsoft.Extensions.Configuration;
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
            var settings = s.GetService<IOptions<AcarsRouterSettings>>()?.Value;
            if (settings is null)
            {
                throw new NullReferenceException();
            }

            return settings.InputProtocol switch
            {
                InputProtocol.Tcp => new TcpInput(s.GetRequiredService<ILogger<TcpInput>>()),
                InputProtocol.Udp => new UdpInput(s.GetRequiredService<ILogger<UdpInput>>()),
                _ => throw new ArgumentOutOfRangeException()
            };
        });

        // AcarsRouterWorker
        services.AddHostedService<AcarsRouterWorker>();
        
        return services;
    }
}