using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Input;

public class TcpInput(ILogger<IInput> logger, string host, int port) : IInput
{
    private const int MaxBufferSize = ushort.MaxValue;
    
    public async Task ReceiveAsync(InputHandler onReceive, CancellationToken cancellationToken = default)
    {
        using var tcpListener = new TcpListener(IPAddress.Parse(host), port);

        tcpListener.Start();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            
            _ = Task.Run(() => HandleClientAsync(onReceive, tcpClient, cancellationToken), cancellationToken);
        }
        
        tcpListener.Stop();
    }

    private async Task HandleClientAsync(InputHandler handler, TcpClient client, CancellationToken cancellationToken = default)
    {
        await using var stream = client.GetStream();
        
        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        logger.LogInformation("Client connected from {RemoteEndPoint}", remoteEndPoint);

        while (client.Connected)
        {
            var buffer = new byte[MaxBufferSize];
            var receivedLength = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

            if (receivedLength == 0 || !client.Connected)
            {
                logger.LogInformation("Client disconnected from {RemoteEndPoint}", remoteEndPoint);
                client.Close();
                break;
            }
            
            await handler.Invoke(buffer, cancellationToken);
        }
    }
}