using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Output;

public sealed class TcpOutput(string host, int port, ILogger<TcpOutput> logger) : IOutput, IDisposable
{
    private static readonly TimeSpan ErrorTimeout = TimeSpan.FromSeconds(4);

    private TcpClient? _client;
    private bool _connected;
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    
    public void Dispose()
    {
        Dispose(true);
    }

    public string EndPoint { get; init; } = $"{host}:{port}";

    public async ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            if (_client is null || !_connected)
            {
                logger.LogInformation("Connecting to {Hostname}:{Port}", host, port);
                _client = new TcpClient();
                await _client.Client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
                _connected = true;
            }

            await _client.GetStream().WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException)
        {
            _connected = false;
            logger.LogWarning(ex,
                "Client failed to connect or got disconnected from {Host}:{Port}, waiting for {ErrorTimeout} seconds!",
                host, port, ErrorTimeout.TotalSeconds);
            await Task.Delay(ErrorTimeout, cancellationToken).ConfigureAwait(false);
            
            logger.LogInformation("Connecting to {Hostname}:{Port}", host, port);
            _client = new TcpClient();
            await _client.Client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
            _connected = true;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        _client?.Dispose();
    }
}