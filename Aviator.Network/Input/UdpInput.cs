using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Input;

public class UdpInput(ILogger<UdpInput> logger, string host, int port) : BaseInput(logger)
{
    public override async Task ReceiveAsync(Func<byte[], CancellationToken, Task> onReceive, CancellationToken cancellationToken = default)
    {
        var udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(host), port));    
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var datagram = await udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            await onReceive.Invoke(datagram.Buffer, cancellationToken).ConfigureAwait(false);   
        }
        
        udpClient.Dispose();
    }
}