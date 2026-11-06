using FoodBridge.Application.Persistence;

namespace FoodBridge.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IListingRepository listings,
        IClaimRequestRepository claimRequests,
        IReservationRepository reservations)
    {
        _context = context;
        Listings = listings;
        ClaimRequests = claimRequests;
        Reservations = reservations;
    }

    public IListingRepository Listings { get; }

    public IClaimRequestRepository ClaimRequests { get; }

    public IReservationRepository Reservations { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
