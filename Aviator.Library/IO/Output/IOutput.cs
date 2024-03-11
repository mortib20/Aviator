using System.Net;

namespace Aviator.Library.IO.Output;

public interface IOutput
{
    public OutputState State { get; }
    public EndPoint EndPoint { get; }
    public Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default);
}