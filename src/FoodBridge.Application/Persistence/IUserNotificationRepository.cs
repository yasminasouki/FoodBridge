using FoodBridge.Domain.Entities;

namespace FoodBridge.Application.Persistence;

public interface IUserNotificationRepository : IRepository<UserNotification>
{
    Task<IReadOnlyList<UserNotification>> ListForUserAsync(string userId, CancellationToken cancellationToken = default);
}
