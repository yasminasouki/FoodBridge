using FoodBridge.Application.Allocation;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence;

public sealed class AllocationGrantQuery : IAllocationGrantQuery
{
    private readonly AppDbContext _context;

    public AllocationGrantQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AllocationGrantDto>> ListAsync(int listingId, CancellationToken cancellationToken = default)
    {
        return await _context.ClaimRequests
            .AsNoTracking()
            .Where(claim => claim.ListingId == listingId)
            .OrderByDescending(claim => claim.PriorityScore)
            .ThenBy(claim => claim.Id)
            .Select(claim => new AllocationGrantDto(
                claim.Id,
                _context.Users.Where(user => user.Id == claim.ReceiverId).Select(user => user.FullName).First(),
                claim.RequestedQuantity,
                claim.AllocatedQuantity,
                claim.PriorityScore,
                claim.Status.ToString(),
                _context.Reservations
                    .Where(reservation => reservation.ClaimRequestId == claim.Id)
                    .Select(reservation => reservation.PickupCode)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);
    }
}
