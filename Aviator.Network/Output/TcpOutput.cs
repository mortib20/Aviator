using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Output;

public sealed class TcpOutput(string host, int port, ILogger<TcpOutput> logger) : IOutput, IDisposable
{
    private static readonly TimeSpan ErrorTimeout = TimeSpan.FromSeconds(4);

    private TcpClient _client = new(host, port);
    private bool _connected;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    
    public void Dispose()
    {
        Dispose(true);
        _connectionLock.Dispose();
    }

    public string EndPoint { get; init; } = $"{host}:{port}";

    public async ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        if (_connectionLock.CurrentCount == 0)
        {
            return;
        }
        
        try
        {
            // Ensure only one reconnect attempt occurs
            if (!_connected)
            {
                await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    if (!_connected) // Check again after acquiring the lock
                    {
                        logger.LogInformation("Connecting to {Hostname}:{Port}", host, port);
                        _client = new TcpClient();
                        await _client.Client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
                        _connected = true;
                    }
                    else
                    {
                        return;
                    }
                }
                finally
                {
                    _connectionLock.Release();
                }
            }

            await _client.GetStream().WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException)
        {
            _connected = false;
            logger.LogWarning(ex, "Client failed to connect or got disconnected from {Host}:{Port}, waiting for {ErrorTimeout} seconds!", host, port, ErrorTimeout.TotalSeconds);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        _client?.Dispose();
    }
}