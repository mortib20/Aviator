namespace Aviator.Network;

public interface IBuilder<out T>
{
    T Create(Protocol protocol, string host, int port);
}