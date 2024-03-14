using System.Reflection;
using Aviator.DependencyInjection;
using Aviator.Library.Acars;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.OpenApi.Models;
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

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo { Title = "Aviator API", Version = "v1" });
        });
        

        var app = builder.Build();
        
        app.UseRouting();
        
        app.UseCors(s =>
        {
            s.AllowAnyHeader();
            s.AllowAnyMethod();
            s.SetIsOriginAllowed(_ => true);
            s.AllowCredentials();
        });
        
        app.AddAviator();

        app.UseSwagger();
        app.UseSwaggerUI(s => s.DocumentTitle = "Aviator API");
        
        await app.RunAsync().ConfigureAwait(false);
    }
}