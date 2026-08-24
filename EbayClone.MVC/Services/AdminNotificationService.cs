using EbayClone.MVC.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EbayClone.MVC.Services;

public sealed class AdminNotificationService(IHubContext<AdminNotificationHub> hubContext)
{
    public Task BroadcastAsync(string message, string type = "success", CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.SendAsync("toast", new { message, type }, cancellationToken);
}
