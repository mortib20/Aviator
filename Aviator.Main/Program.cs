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

        builder.Services.AddCors();
        
        builder.WebHost.ConfigureKestrel(kestrel => kestrel.ListenAnyIP(21001));
        
        builder.Services.AddSignalR();
        
        builder.AddAviator();

        var host = builder.Build();
        host.UseRouting();
        host.UseCors(s =>
        {
            s.AllowAnyHeader();
            s.AllowAnyMethod();
            s.SetIsOriginAllowed(_ => true);
            s.AllowCredentials();
        });

        host.MapHub<AcarsHub>("/hub/acars");
        host.MapMetrics();
        host.AddAviator();
        
        await host.RunAsync();
    }
}