using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Exceptions;

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

    public async Task<IReadOnlyList<NotificationDto>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var items = await _notifications.ListForUserAsync(userId, cancellationToken);
        return items
            .Select(item => new NotificationDto(item.Id, item.Message, item.ListingId, item.CreatedAt, item.ReadAt is not null))
            .ToList();
    }

    public async Task<NotificationDto> MarkReadAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var notice = await _notifications.GetByIdAsync(id, cancellationToken);
        if (notice is null || notice.UserId != userId)
        {
            throw new NotFoundException("That notification was not found.");
        }

        notice.MarkRead(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new NotificationDto(notice.Id, notice.Message, notice.ListingId, notice.CreatedAt, true);
    }
}
