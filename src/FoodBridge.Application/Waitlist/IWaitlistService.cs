using FoodBridge.Application.Reservations;

namespace FoodBridge.Application.Waitlist;

public interface IWaitlistService
{
    Task JoinAsync(string userId, int listingId, int quantity, CancellationToken cancellationToken = default);

    Task LeaveAsync(string userId, int listingId, CancellationToken cancellationToken = default);
}
