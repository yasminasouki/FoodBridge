namespace FoodBridge.Application.Notifications;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> ListAndMarkReadAsync(string userId, CancellationToken cancellationToken = default);
}
