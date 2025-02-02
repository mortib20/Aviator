using System.Text.Json.Nodes;
using Aviator.Acars.Database;
using Aviator.Acars.Entities;
using Aviator.Acars.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsService(ILogger<AcarsService> logger, AcarsIoManager ioManager, IAcarsMetrics metrics, IAcarsDatabase database)
    : BackgroundService
{
    private const int MinBytes = 128;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {This}", this);

        try
        {
            await ioManager.StartInputAsync(OnReceivedAsync, stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while running Input");
        }
    }

    private async Task OnReceivedAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        if (bytes.Length < MinBytes)
        {
            logger.LogWarning("Received payload to small!");
            return;
        }
        
        JsonNode jsonAcars;
        try
        {
            jsonAcars = JsonNode.Parse(bytes) ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid JSON payload, Ignoring...");
            return;
        }

        SourceType sourceType;
        try
        {
            sourceType = SourceTypeFinder.Detect(jsonAcars) ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to detect Frame type, ignoring...");
            return;
        }

        try
        {
            await ioManager.WriteToTypeAsync(sourceType, bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while trying to write to output of {AcarsType}", sourceType);
        }

        try
        {
            await database.InsertAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save bytes in database!");
        }
        
        var airFrame = AirFrameConverter.FromType(bytes, sourceType);


        if (airFrame is null) return;

        if (SourceTypeFinder.HasAcars(jsonAcars))
        {
            airFrame.FrameType = FrameType.Acars;
        }

        await metrics.IncreaseAsync(airFrame, cancellationToken).ConfigureAwait(false);
    }
}