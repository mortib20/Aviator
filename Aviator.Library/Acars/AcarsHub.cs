using Microsoft.AspNetCore.SignalR;

namespace Aviator.Library.Acars;

public class AcarsHub : Hub
{
    public async Task SendMessage(AcarsType type, byte[] buffer) => await Clients.All.SendAsync(type.ToString(), buffer);
}