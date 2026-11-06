using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ReceiverProfileRepository : IReceiverProfileRepository
{
    private readonly AppDbContext _context;

    public ReceiverProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ReceiverProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _context.ReceiverProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ReceiverProfile>> GetByUserIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        return await _context.ReceiverProfiles
            .AsNoTracking()
            .Where(profile => userIds.Contains(profile.UserId))
            .ToListAsync(cancellationToken);
    }
}
