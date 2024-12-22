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
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {This}", this);

        try
        {
            await ioManager.StartInputAsync(OnReceivedAsync, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // no error
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while running Input");
        }
    }

    private async Task OnReceivedAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        if (bytes.Length < MinBytes)
            return;

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

        AcarsType acarsType;
        try
        {
            acarsType = AcarsTypeFinder.Detect(jsonAcars) ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to detect ACARS type, Ignoring...");
            return;
        }

        await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await ioManager.WriteToTypeAsync(acarsType, bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while trying to write to output of {AcarsType}", acarsType);
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        try
        {
            await database.InsertAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save bytes in database!");
        }

        if (!AcarsTypeFinder.HasAcars(jsonAcars))
        {
            return;
        }
        
        var basicAcars = AcarsConverter.BasicAcarsFromType(bytes, acarsType);

        if (basicAcars is null) return;

        await metrics.IncreaseAsync(acarsType, basicAcars, cancellationToken).ConfigureAwait(false);
    }
}