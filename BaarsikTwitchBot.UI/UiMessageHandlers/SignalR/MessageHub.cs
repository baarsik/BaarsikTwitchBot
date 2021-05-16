using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BaarsikTwitchBot.UI.UiMessageHandlers.SignalR
{
    public class MessageHub : Hub
    {
        public async Task Send(object message)
        {
            await Clients.All.SendAsync("Send", message);
        }
    }
}