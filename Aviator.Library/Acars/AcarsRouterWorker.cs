using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aviator.Library.Acars.Settings;
using Aviator.Library.Acars.Types.Acars;
using Aviator.Library.Acars.Types.Hfdl;
using Aviator.Library.Acars.Types.Jaero;
using Aviator.Library.Acars.Types.VDL2;
using Aviator.Library.IO.Input;
using Aviator.Library.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aviator.Library.Acars;

public class AcarsRouterWorker(
    AcarsOutputManager outputManager,
    AviatorMetrics metrics,
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

    private async void Handler(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            if (buffer.Length < MinPacketLength) return;

            var json = JsonNode.Parse(buffer);

            if (json is null) return;

            var acarsType = AcarsTypeFinder.Detect(json);

            if (acarsType is null) return;

            // Ignore all non acars
            if (!AcarsTypeFinder.HasAcars(json)) return;

            var basicAcars = BasicAcarsFromType(buffer, acarsType, out var sigLevel, out var noiseLevel);

            // Dont send null
            if (basicAcars is null)
                return;

            await outputManager.SendAsync((AcarsType)acarsType, buffer, cancellationToken).ConfigureAwait(false);
            await outputManager.SendAcarsHub(basicAcars, cancellationToken).ConfigureAwait(false);

            metrics
                .IncreaseMessagesTotal(basicAcars);

            metrics.AddSignalLevel(basicAcars, sigLevel);

            metrics.AddNoiseLevel(basicAcars, noiseLevel);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured");
        }
    }

    private static BasicAcars? BasicAcarsFromType(byte[] buffer, [DisallowNull] AcarsType? acarsType,
        out double? sigLevel, out double? noiseLevel)
    {
        sigLevel = null;
        noiseLevel = null;
        switch (acarsType)
        {
            case AcarsType.Aero:
                var jaero = JsonSerializer.Deserialize<Jaero>(buffer);
                if (jaero is null) break;
                return AcarsConverter.ConvertAero(jaero);
            case AcarsType.Vdl2:
                var vdl2 = JsonSerializer.Deserialize<DumpVdl2>(buffer);
                if (vdl2 is null) break;
                sigLevel = vdl2.vdl2.sig_level;
                noiseLevel = vdl2.vdl2.noise_level;
                return AcarsConverter.ConvertDumpVdl2(vdl2);
            case AcarsType.Hfdl:
                var hfdl = JsonSerializer.Deserialize<DumpHfdl>(buffer);
                if (hfdl is null) break;
                sigLevel = hfdl.hfdl.sig_level;
                noiseLevel = hfdl.hfdl.noise_level;
                return AcarsConverter.ConvertDumpHfdl(hfdl);
            case AcarsType.Acars:
                var acars = JsonSerializer.Deserialize<Acarsdec>(buffer);
                if (acars is null) break;
                sigLevel = acars.level;
                return AcarsConverter.ConvertAcarsdec(acars);
            case AcarsType.Iridium:
                // not implemented
                break;
            default:
                throw new ArgumentOutOfRangeException(acarsType.ToString());
        }

        return null;
    }
}