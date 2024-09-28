using Aviator.Acars.Config;
using Aviator.Network.Input;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsIoManager(ILogger<AcarsIoManager> logger, AcarsConfig config, InputBuilder inputBuilder)
{
    private readonly BaseInput _input = inputBuilder.Create(config.Input.Protocol, config.Input.Host, config.Input.Port);

    public Task StartInputAsync(Func<byte[], CancellationToken, Task> onReceivedAsync, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Start Input on {Endpoint}", config.Input);
        
        return _input.ReceiveAsync(onReceivedAsync, cancellationToken);
    }
}