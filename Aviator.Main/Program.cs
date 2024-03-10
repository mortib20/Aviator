using Aviator.DependencyInjection;
using Aviator.Library.Acars;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Prometheus;

namespace Aviator.Main;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        Metrics.SuppressDefaultMetrics();
        
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(s =>
        {
            s.AddDefaultPolicy(p =>
            {
                p.AllowCredentials();
                p.AllowAnyOrigin();
                p.AllowAnyMethod();
            });
        });
        
        builder.WebHost.ConfigureKestrel(kestrel => kestrel.ListenAnyIP(21001));
        
        builder.Services.AddSignalR();
        
        builder.AddAviator();

        var host = builder.Build();
        host.UseRouting();

        host.MapHub<AcarsHub>("/hub/acars");
        host.MapMetrics();
        
        await host.RunAsync();
    }
}