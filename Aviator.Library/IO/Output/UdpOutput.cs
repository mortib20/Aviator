using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Library.IO.Output;

public class UdpOutput(ILogger logger, EndPoint dnsEndPoint) : AbstractOutput(dnsEndPoint)
{
    private readonly UdpClient _client = new()
    {
        EnableBroadcast = true // Because, Why not?!?
    };

    private bool _firstMessage = true;

    public override async Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_firstMessage)
            {
                _firstMessage = false;
                logger.LogInformation("Connected");
                StateRunning();
            }

            await _client.SendAsync(buffer, EndPoint.Host, EndPoint.Port, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to send, {msg}", e);
            StateConfigured();
        }
    }
}