using Microsoft.AspNetCore.SignalR;

namespace EbayClone.MVC.Hubs;

public sealed class AdminNotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var session = Context.GetHttpContext()?.Session;
        if (string.IsNullOrWhiteSpace(session?.GetString("AdminToken")))
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }
}
