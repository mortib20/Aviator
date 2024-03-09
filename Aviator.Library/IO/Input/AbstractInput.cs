using Microsoft.Extensions.Logging;

namespace Aviator.Library.IO.Input;

public abstract class AbstractInput(ILogger logger) : IInput
{
    public virtual Task StartReceiveAsync(int port, Action<byte[]> handler, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listening on {port}", port);
        return Task.CompletedTask;
    }
}