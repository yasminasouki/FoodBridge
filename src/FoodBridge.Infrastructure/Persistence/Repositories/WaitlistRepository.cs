using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class WaitlistRepository : Repository<WaitlistEntry>, IWaitlistRepository
{
    private readonly AppDbContext _context;

    public WaitlistRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<WaitlistEntry?> FindAsync(int listingId, string receiverId, CancellationToken cancellationToken = default)
    {
        return _context.WaitlistEntries.FirstOrDefaultAsync(
            entry => entry.ListingId == listingId && entry.ReceiverId == receiverId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<WaitlistEntry>> ListWaitingAsync(int listingId, CancellationToken cancellationToken = default)
    {
        return await _context.WaitlistEntries
            .Where(entry => entry.ListingId == listingId && entry.Status == WaitlistStatus.Waiting)
            .OrderBy(entry => entry.JoinedAt)
            .ThenBy(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }
}
