using Aviator.Network.Input;
using Aviator.Network.Output;

namespace Aviator.Main.DependencyInjection;

public static class NetworkExtension
{
    public static WebApplicationBuilder AddNetworkUtilities(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<InputBuilder>();
        builder.Services.AddSingleton<OutputBuilder>();

        return builder;
    }
}