using System.Text.Json;
using System.Text.Json.Nodes;
using Aviator.Library.Acars.Settings;
using Aviator.Library.IO.Input;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aviator.Library.Acars;

public class AcarsRouterWorker(IHubContext<AcarsHub> acarsHub, ILogger<AcarsRouterWorker> logger, ILoggerFactory loggerFactory, IOptions<AcarsRouterSettings> options, IInput input) : BackgroundService
{
    private readonly AcarsRouterSettings _settings = options.Value;
    private readonly AcarsRouterOutputManager _outputManager = new(loggerFactory, options);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await input.StartReceiveAsync(_settings.InputPort, Handler, stoppingToken).ConfigureAwait(false);
    }

    private void Handler(byte[] buffer)
    {
        try
        {
            var json = JsonNode.Parse(buffer);

            if (json is null)
            {
                return;
            }

            var type = AcarsTypeFinder.Detect(json);

            if (type is null)
            {
                return;
            }
            
            _outputManager.SendAsync((AcarsType)type, buffer).Wait();

            if (AcarsTypeFinder.HasAcars(json))
            {
                acarsHub.Clients.All.SendAsync("data", System.Text.Encoding.UTF8.GetString(buffer));
            }
        }
        catch (Exception e)
        {
            logger.LogError("{message}", e);
        }
    }
}