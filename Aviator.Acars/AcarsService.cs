using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsService(ILogger<AcarsService> logger,  AcarsIoManager ioManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {This}", this);
        
        await ioManager.StartInputAsync(OnReceivedAsync, stoppingToken).ConfigureAwait(false);
    }

    private static async Task OnReceivedAsync(byte[] bytes, CancellationToken cancellationToken)
    { 
        if (bytes.Length < 128)
        {
            return;
        }
        
        Console.WriteLine($"Received {bytes.Length} bytes");
        await Task.CompletedTask;
    }
}