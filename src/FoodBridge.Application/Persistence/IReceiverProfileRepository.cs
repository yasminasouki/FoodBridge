using FoodBridge.Domain.Entities;

namespace FoodBridge.Application.Persistence;

public interface IReceiverProfileRepository
{
    Task<ReceiverProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReceiverProfile>> GetByUserIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReceiverProfile>> GetTrackedByUserIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default);
}
