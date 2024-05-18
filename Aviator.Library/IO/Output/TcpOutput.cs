using System.Net;
using System.Net.Sockets;
using System.Timers;
using Aviator.Library.Metrics;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Aviator.Library.IO.Output;

public class TcpOutput(ILogger logger, EndPoint endPoint, AviatorMetrics metrics) : AbstractOutput(endPoint, metrics)
{
    private TcpClient _client = new();
    private readonly Timer _timer = new(TimeSpan.FromSeconds(5));
    private bool _firstMessage = true;

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_client.Connected)
        {
            _timer.Interval = 1000;
            StateToRunning();
            return;
        }

        _timer.Interval += TimeSpan.FromSeconds(2).Milliseconds;
            
        logger.LogInformation("Reconnecting..., {Interval}", TimeSpan.FromMilliseconds(_timer.Interval).ToString());
            
        ConnectAsync().Wait();
    }

    private async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _client = new TcpClient();
            StateToInitialized();
                
            await _client.ConnectAsync(EndPoint.Host, EndPoint.Port, cancellationToken)
                .ConfigureAwait(false);

            if (_client.Connected)
            {
                logger.LogInformation("Connected");
                StateToRunning();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect");
            LastError = DateTime.Now;
            StateToStopped();
        }
    }

    public override async Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        if (_firstMessage)
        {
            _firstMessage = false;
            await ConnectAsync(cancellationToken).ConfigureAwait(false);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }
            
        if (!_client.Connected)
        {
            return;
        }

        try
        {
            await _client.GetStream().WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write");
            LastError = DateTime.Now;
            StateToStopped();
        }
    }
}