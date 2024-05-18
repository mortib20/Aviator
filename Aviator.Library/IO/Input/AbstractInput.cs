using Microsoft.Extensions.Logging;

namespace Aviator.Library.IO.Input;

public abstract class AbstractInput(ILogger logger) : IInput
{
    public virtual Task StartReceiveAsync(int port, Action<byte[], CancellationToken> handler, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listening on {Port}", port);
        return Task.CompletedTask;
    }
}