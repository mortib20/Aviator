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
    private bool _hadError;

    public override async Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_firstMessage || _hadError)
            {
                _firstMessage = false;
                if (_hadError)
                {   
                    _hadError = false;
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                }
                logger.LogInformation("Connected");
                StateToRunning();
            }
            
            await _client.SendAsync(buffer, EndPoint.Host, EndPoint.Port, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to send, {msg}", ex.Message);
            _hadError = true;
            LastError = DateTime.Now;
            StateToStopped();
        }
    }
}