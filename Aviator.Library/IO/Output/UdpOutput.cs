using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Library.IO.Output;

public class UdpOutput(ILogger logger, DnsEndPoint dnsEndPoint) : AbstractOutput
{
    private readonly UdpClient _client = new()
    {
        EnableBroadcast = true
    };

    public override bool Connected => _client.Client.Connected;

    public override async Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        await _client.SendAsync(buffer, new IPEndPoint(IPAddress.Parse("127.255.255.255"), 2000), cancellationToken).ConfigureAwait(false);
    }
}