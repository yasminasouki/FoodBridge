using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<string, int>> SumCollectedSinceAsync(
        IReadOnlyCollection<string> receiverIds,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default)
    {
        if (receiverIds.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        var rows = await _context.Reservations
            .AsNoTracking()
            .Where(reservation =>
                receiverIds.Contains(reservation.ReceiverId) &&
                reservation.Status == ReservationStatus.Collected &&
                reservation.CollectedAt >= sinceUtc)
            .GroupBy(reservation => reservation.ReceiverId)
            .Select(group => new { ReceiverId = group.Key, Total = group.Sum(reservation => reservation.Quantity) })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.ReceiverId, row => row.Total);
    }
}
