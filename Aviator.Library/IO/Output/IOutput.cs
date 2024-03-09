namespace Aviator.Library.IO.Output;

public interface IOutput
{
    public Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default);
}