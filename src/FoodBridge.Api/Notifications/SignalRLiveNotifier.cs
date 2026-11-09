using FoodBridge.Application.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace FoodBridge.Api.Notifications;

public sealed class SignalRLiveNotifier : ILiveNotifier
{
    private readonly IHubContext<NotificationsHub> _hub;

    public SignalRLiveNotifier(IHubContext<NotificationsHub> hub)
    {
        _hub = hub;
    }

    public Task PushAsync(string userId, int id, string message, int? listingId, CancellationToken cancellationToken = default)
    {
        return _hub.Clients.User(userId).SendAsync("notice", new { id, message, listingId }, cancellationToken);
    }
}
