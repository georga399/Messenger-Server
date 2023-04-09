using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;   
namespace Messenger.Hubs;

[Authorize]
class MessengerHub: Hub
{
    public async Task Send(string message)
    {
        await this.Clients.All.SendAsync("Recieve", message);
    }
    
}