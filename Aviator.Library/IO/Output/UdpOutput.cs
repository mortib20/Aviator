using System.Net.Sockets;
using Aviator.Library.Metrics;
using Microsoft.Extensions.Logging;

namespace Aviator.Library.IO.Output;

public class UdpOutput(ILogger logger, EndPoint dnsEndPoint, AviatorMetrics metrics) : AbstractOutput(dnsEndPoint, metrics)
{
    private readonly UdpClient _client = new();

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
            logger.LogError(ex, "Failed to send");
            _hadError = true;
            LastError = DateTime.Now;
            StateToStopped();
        }
    }

    protected override void Dispose(bool disposing)
    {
        _client.Close();
        _client.Dispose();
        base.Dispose(disposing);
    }
}