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

    private bool _firstMessage = true;

    public override bool Connected => _client.Client.Connected;

    public override async Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        if (_firstMessage)
        {
            _firstMessage = false;
            logger.LogInformation("Connected");
        }

        await _client.SendAsync(buffer, dnsEndPoint.Host, dnsEndPoint.Port, cancellationToken).ConfigureAwait(false);
    }
}