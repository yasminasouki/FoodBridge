using FoodBridge.Domain.Entities;

namespace FoodBridge.Application.Persistence;

public interface IShopRepository
{
    Task<Shop?> GetByOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default);

    Task<string?> GetOwnerUserIdAsync(int shopId, CancellationToken cancellationToken = default);
}
