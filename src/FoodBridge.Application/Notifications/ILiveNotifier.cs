namespace FoodBridge.Application.Notifications;

public interface ILiveNotifier
{
    Task PushAsync(string userId, int id, string message, int? listingId, CancellationToken cancellationToken = default);
}
