namespace FoodBridge.Application.Notifications;

public interface IUserNotifier
{
    Task NotifyAsync(string userId, string message, int? listingId, CancellationToken cancellationToken = default);

    Task PushQueuedAsync(CancellationToken cancellationToken = default);
}
