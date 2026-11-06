namespace FoodBridge.Application.Persistence;

public interface IUnitOfWork
{
    IListingRepository Listings { get; }

    IClaimRequestRepository ClaimRequests { get; }

    IReservationRepository Reservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
