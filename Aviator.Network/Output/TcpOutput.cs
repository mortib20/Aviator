using System.Net.Sockets;

namespace Aviator.Network.Output;

public class TcpOutput(string host, int port) : IOutput
{
    private readonly TcpClient _client = new(host, port);
    
    public async Task WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = _client.GetStream();

            await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Reconnect or check what error and handle depending
            throw;
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}