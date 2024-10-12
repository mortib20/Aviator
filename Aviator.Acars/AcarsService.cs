using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsService(ILogger<AcarsService> logger,  AcarsIoManager ioManager) : BackgroundService
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
        
        await ioManager.WriteToTypeAsync(AcarsType.Vdl2, bytes, cancellationToken).ConfigureAwait(false);
    }
}