using System.Text.Json.Nodes;
using Aviator.Acars.Entities;
using Aviator.Acars.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsService(ILogger<AcarsService> logger,  AcarsIoManager ioManager, IAcarsMetrics metrics) : BackgroundService
{
    private const int MinBytes = 0; 
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {This}", this);
        
        await ioManager.StartInputAsync(OnReceivedAsync, stoppingToken).ConfigureAwait(false);
    }

    private async Task OnReceivedAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        if (bytes.Length < MinBytes) // FIXME: After testing enable to prevent useless spamming
        {
            return;
        }
        
        var acars = JsonNode.Parse(bytes);

        if (acars is null)
        {
            return;
        }
        
        var acarsType = AcarsTypeFinder.Detect(acars);

        if (acarsType is null)
        {
            return;
        }
        
        await ioManager.WriteToTypeAsync((AcarsType)acarsType, bytes, cancellationToken).ConfigureAwait(false);

        var b = new BasicAcars
        {
            Type = (AcarsType)acarsType,
            Address = "3214",
            Channel = "1234",
            Time = default,
            Message = string.Empty
        };
        
        await metrics.Increase((AcarsType)acarsType, b, cancellationToken);
    }
}