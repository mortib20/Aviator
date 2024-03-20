using System.Net;

namespace Aviator.Library.IO.Output;

public interface IOutput
{
    public bool Enabled { get; }
    public OutputState State { get; }
    public EndPoint EndPoint { get; }
    public DateTime? LastError { get; }
    public Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default);
}