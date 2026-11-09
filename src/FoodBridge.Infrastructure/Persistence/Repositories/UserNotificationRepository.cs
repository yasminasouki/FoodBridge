using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class UserNotificationRepository : Repository<UserNotification>, IUserNotificationRepository
{
    private readonly AppDbContext _context;

    public UserNotificationRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserNotification>> ListForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserNotifications
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(30)
            .ToListAsync(cancellationToken);
    }
}
