namespace FoodBridge.Application.Notifications;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> ListAsync(string userId, CancellationToken cancellationToken = default);

    Task<NotificationDto> MarkReadAsync(string userId, int id, CancellationToken cancellationToken = default);
}
