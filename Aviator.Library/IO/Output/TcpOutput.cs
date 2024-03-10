using System.Net;
using System.Net.Sockets;
using System.Timers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Aviator.Library.IO.Output
{
    public class TcpOutput(ILogger logger, DnsEndPoint dnsEndPoint) : AbstractOutput
    {
        private TcpClient _client = new();
        private readonly Timer _timer = new(TimeSpan.FromSeconds(5));
        private bool _firstMessage = true;
        private bool _disconnected;

        public override bool Connected => _client.Connected;

        private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_client.Connected)
            {
                _timer.Interval = 1000;
                return;
            }

            _timer.Interval += TimeSpan.FromSeconds(2).Milliseconds;
            
            ConnectAsync().Wait();
        }

        private async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_disconnected)
                {
                    _disconnected = false;
                    _client = new TcpClient();
                }
                
                await _client.ConnectAsync(dnsEndPoint.Host, dnsEndPoint.Port, cancellationToken)
                    .ConfigureAwait(false);

                if (_client.Connected)
                {
                    logger.LogInformation("Connected");
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to connect, {message}", ex.Message);
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
                logger.LogError("Failed to write, {message}", ex.Message);
                _disconnected = true;
            }
        }
    }
}
