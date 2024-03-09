using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Library.IO.Input;

public class TcpInput(ILogger logger) : AbstractInput(logger)
{
    private const int BufferSize = 2048;
    
    public override async Task StartReceiveAsync(int port, Action<byte[]> handler, CancellationToken cancellationToken = default)
    {
        await base.StartReceiveAsync(port, handler, cancellationToken).ConfigureAwait(false);
        
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            _ = Task.Run(() => HandleClientAsync(handler, client, cancellationToken), cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task HandleClientAsync(Action<byte[]> handler, TcpClient client, CancellationToken cancellationToken = default)
    {
        await using var stream = client.GetStream();
            
        var remoteEndpoint = client.Client.RemoteEndPoint?.ToString();
        logger.LogInformation("{client} connected", remoteEndpoint);

        while (client.Connected)
        {
            var receiveBuffer = new byte[BufferSize];
            var length = await stream.ReadAsync(receiveBuffer, cancellationToken).ConfigureAwait(false);
                
            if (length == 0 || !client.Connected)
            {
                logger.LogInformation("{client} connection closed", remoteEndpoint);
                client.Close();
                break;    
            }
                
            handler.Invoke(receiveBuffer[..length]);
        }
    }
}