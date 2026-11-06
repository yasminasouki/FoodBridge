using FoodBridge.Domain.Entities;

namespace FoodBridge.Application.Persistence;

public interface IClaimRequestRepository : IRepository<ClaimRequest>
{
    Task<bool> ExistsAsync(int listingId, string receiverId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimRequest>> ListPendingForListingAsync(
        int listingId,
        CancellationToken cancellationToken = default);
}
