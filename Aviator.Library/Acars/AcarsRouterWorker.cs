using System.Text.Json;
using System.Text.Json.Nodes;
using Aviator.Library.Acars.Settings;
using Aviator.Library.Acars.Types.Acars;
using Aviator.Library.Acars.Types.Hfdl;
using Aviator.Library.Acars.Types.Jaero;
using Aviator.Library.Acars.Types.VDL2;
using Aviator.Library.IO.Input;
using Aviator.Library.Metrics;
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
            BasicAcars? basicAcars = null;

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

            if (!AcarsTypeFinder.HasAcars(json)) return;
            // Ignore all non acars
            
            switch (type)
            {
                case AcarsType.Aero:
                    var jaero = JsonSerializer.Deserialize<Jaero>(buffer);
                    if (jaero is null)
                    {
                        break;
                    }
                    basicAcars = AcarsConverter.ConvertAero(jaero);
                    break;
                case AcarsType.Vdl2:
                    var vdl2 = JsonSerializer.Deserialize<DumpVdl2>(buffer);
                    if (vdl2 is null)
                    {
                        break;
                    }
                    basicAcars = AcarsConverter.ConvertDumpVdl2(vdl2);
                    break;
                case AcarsType.Hfdl:
                    var hfdl = JsonSerializer.Deserialize<DumpHfdl>(buffer);
                    if (hfdl is null)
                    {
                        break;
                    }
                    basicAcars = AcarsConverter.ConvertDumpHfdl(hfdl);
                    break;
                case AcarsType.Acars:
                    var acars = JsonSerializer.Deserialize<Acarsdec>(buffer);
                    if (acars is null)
                    {
                        break;
                    }
                    basicAcars = AcarsConverter.ConvertAcarsdec(acars);
                    break;
                case AcarsType.Iridium:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (basicAcars is not null)
            {
                AviatorRouterMetrics
                    .ReceivedMessagesTotal
                    .WithLabels([basicAcars.Type, basicAcars.Freq])
                    .Inc();

                acarsHub.Clients.All.SendAsync("acars", JsonSerializer.Serialize(basicAcars));
            }
            
            
            acarsHub.Clients.All.SendAsync("data", System.Text.Encoding.UTF8.GetString(buffer));
        }
        catch (Exception e)
        {
            logger.LogError("{message}", e);
        }
    }
}