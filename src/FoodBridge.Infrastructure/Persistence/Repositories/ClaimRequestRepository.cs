using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ClaimRequestRepository : Repository<ClaimRequest>, IClaimRequestRepository
{
    private readonly AppDbContext _context;

    public ClaimRequestRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(int listingId, string receiverId, CancellationToken cancellationToken = default)
    {
        return _context.ClaimRequests.AnyAsync(
            request => request.ListingId == listingId && request.ReceiverId == receiverId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimRequest>> ListPendingForListingAsync(
        int listingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimRequests
            .Where(request => request.ListingId == listingId && request.Status == ClaimRequestStatus.Pending)
            .ToListAsync(cancellationToken);
    }
}
