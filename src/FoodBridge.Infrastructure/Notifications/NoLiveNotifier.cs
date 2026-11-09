using FoodBridge.Application.Notifications;

namespace FoodBridge.Infrastructure.Notifications;

public sealed class NoLiveNotifier : ILiveNotifier
{
    public Task PushAsync(string userId, int id, string message, int? listingId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
