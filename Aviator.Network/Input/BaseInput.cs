using Microsoft.Extensions.Logging;

namespace Aviator.Network.Input;

public abstract class BaseInput(ILogger<BaseInput> logger)
{
    public abstract Task ReceiveAsync(Func<byte[], CancellationToken, Task> onReceive, CancellationToken cancellationToken = default);
}