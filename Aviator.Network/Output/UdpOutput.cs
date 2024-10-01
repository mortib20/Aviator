using System.Net.Sockets;

namespace Aviator.Network.Output;

public class UdpOutput(string host, int port) : IOutput
{
    public async Task WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            using var udpClient = new UdpClient();

            await udpClient.SendAsync(buffer, buffer.Length).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // No handling needed UDP is fire and forget
            throw;
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}