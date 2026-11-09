using FoodBridge.Application.Notifications;
using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Entities;

namespace FoodBridge.Infrastructure.Notifications;

public sealed class UserNotifier : IUserNotifier
{
    private readonly IUserNotificationRepository _notifications;
    private readonly ILiveNotifier _live;
    private readonly IClock _clock;
    private readonly List<UserNotification> _queued = [];

    public UserNotifier(IUserNotificationRepository notifications, ILiveNotifier live, IClock clock)
    {
        _notifications = notifications;
        _live = live;
        _clock = clock;
    }

    public async Task NotifyAsync(
        string userId,
        string message,
        int? listingId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var notice = new UserNotification(userId, message, listingId, _clock.UtcNow);
        await _notifications.AddAsync(notice, cancellationToken);
        _queued.Add(notice);
    }

    public async Task PushQueuedAsync(CancellationToken cancellationToken = default)
    {
        if (_queued.Count == 0)
        {
            return;
        }

        var batch = _queued.ToArray();
        _queued.Clear();
        foreach (var notice in batch)
        {
            await _live.PushAsync(notice.UserId, notice.Id, notice.Message, notice.ListingId, cancellationToken);
        }
    }
}
