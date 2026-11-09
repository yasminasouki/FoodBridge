using FoodBridge.Domain.Entities;

namespace FoodBridge.Application.Persistence;

public interface IWaitlistRepository : IRepository<WaitlistEntry>
{
    Task<WaitlistEntry?> FindAsync(int listingId, string receiverId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WaitlistEntry>> ListWaitingAsync(int listingId, CancellationToken cancellationToken = default);
}
