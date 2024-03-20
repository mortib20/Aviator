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

public class AcarsRouterWorker(
    AcarsOutputManager outputManager,
    IHubContext<AcarsHub> acarsHub,
    ILogger<AcarsRouterWorker> logger,
    IOptions<AcarsRouterSettings> options,
    IInput input) : BackgroundService
{
    private const int MinPacketLength = 100;
    private readonly AcarsRouterSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await input.StartReceiveAsync(_settings.InputPort, Handler, stoppingToken).ConfigureAwait(false);
    }

    private void Handler(byte[] buffer)
    {
        try
        {
            if (buffer.Length < MinPacketLength) return;

            var json = JsonNode.Parse(buffer);
            BasicAcars? basicAcars = null;
            double? sigLevel = null;
            double? noiseLevel = null;

            if (json is null) return;

            var acarsType = AcarsTypeFinder.Detect(json);

            if (acarsType is null) return;

            outputManager.SendAsync((AcarsType)acarsType, buffer).Wait();
            acarsHub.Clients.All.SendAsync("raw", JsonSerializer.Serialize(buffer));

            // Ignore all non acars
            if (!AcarsTypeFinder.HasAcars(json)) return;

            switch (acarsType)
            {
                case AcarsType.Aero:
                    var jaero = JsonSerializer.Deserialize<Jaero>(buffer);
                    if (jaero is null) break;
                    basicAcars = AcarsConverter.ConvertAero(jaero);
                    break;
                case AcarsType.Vdl2:
                    var vdl2 = JsonSerializer.Deserialize<DumpVdl2>(buffer);
                    if (vdl2 is null) break;
                    basicAcars = AcarsConverter.ConvertDumpVdl2(vdl2);
                    sigLevel = vdl2.vdl2.sig_level;
                    noiseLevel = vdl2.vdl2.noise_level;
                    break;
                case AcarsType.Hfdl:
                    var hfdl = JsonSerializer.Deserialize<DumpHfdl>(buffer);
                    if (hfdl is null) break;
                    basicAcars = AcarsConverter.ConvertDumpHfdl(hfdl);
                    sigLevel = hfdl.hfdl.sig_level;
                    noiseLevel = hfdl.hfdl.noise_level;
                    break;
                case AcarsType.Acars:
                    var acars = JsonSerializer.Deserialize<Acarsdec>(buffer);
                    if (acars is null) break;
                    basicAcars = AcarsConverter.ConvertAcarsdec(acars);
                    sigLevel = acars.level;
                    break;
                case AcarsType.Iridium:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (basicAcars is null) // Dont send null
                return;

            acarsHub.Clients.All.SendAsync("acars", JsonSerializer.Serialize(basicAcars));

            AviatorRouterMetrics
                .IncReceivedMessagesTotal(basicAcars);

            if (sigLevel is not null) AviatorRouterMetrics.AddSigLevel(basicAcars, (double)sigLevel);

            if (noiseLevel is not null) AviatorRouterMetrics.AddNoiseLevel(basicAcars, (double)noiseLevel);
        }
        catch (Exception e)
        {
            logger.LogError("{message}", e);
        }
    }
}