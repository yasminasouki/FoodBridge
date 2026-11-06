using FoodBridge.Domain.Entities;

namespace FoodBridge.Application.Persistence;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IReadOnlyDictionary<string, int>> SumCollectedSinceAsync(
        IReadOnlyCollection<string> receiverIds,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default);
}
