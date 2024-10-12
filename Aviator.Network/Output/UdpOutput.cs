using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Output;

public sealed class UdpOutput(string host, int port, ILogger<UdpOutput> logger) : IOutput
{
    public async ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            using var udpClient = new UdpClient(host, port);

            await udpClient.SendAsync(buffer, buffer.Length).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "We call it unhandled error.");
        }
    }
}