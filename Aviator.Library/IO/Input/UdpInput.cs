using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Library.IO.Input;

public class UdpInput(ILogger logger) : AbstractInput(logger)
{
    public override async Task StartReceiveAsync(int port, Action<byte[], CancellationToken> handler, CancellationToken cancellationToken = default)
    {
        await base.StartReceiveAsync(port, handler, cancellationToken).ConfigureAwait(false);
        using var client = new UdpClient(port);

        while (!cancellationToken.IsCancellationRequested)
        {
            var data = await client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            handler.Invoke(data.Buffer, cancellationToken);
        }
    }
}