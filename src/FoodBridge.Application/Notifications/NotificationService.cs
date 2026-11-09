using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;

namespace FoodBridge.Application.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly IUserNotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public NotificationService(IUserNotificationRepository notifications, IUnitOfWork unitOfWork, IClock clock)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<IReadOnlyList<NotificationDto>> ListAndMarkReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var items = await _notifications.ListForUserAsync(userId, cancellationToken);
        foreach (var item in items)
        {
            item.MarkRead(_clock.UtcNow);
        }

        if (items.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return items
            .Select(item => new NotificationDto(item.Id, item.Message, item.ListingId, item.CreatedAt, item.ReadAt is not null))
            .ToList();
    }
}
