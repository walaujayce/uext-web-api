using Microsoft.AspNetCore.SignalR;

namespace UMonitorWebAPI.Hubs
{
    public sealed class NotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string topic, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", $"{topic}", $"{message}");
        }

    }
}
