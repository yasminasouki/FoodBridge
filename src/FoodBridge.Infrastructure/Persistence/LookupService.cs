using FoodBridge.Application.Listings;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence;

public sealed class LookupService : ILookupService
{
    private readonly AppDbContext _context;

    public LookupService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LookupDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var areas = await _context.Areas
            .AsNoTracking()
            .OrderBy(area => area.Name)
            .Select(area => new LookupItemDto(area.Id, area.Name, area.District))
            .ToListAsync(cancellationToken);
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new LookupItemDto(category.Id, category.Name, null))
            .ToListAsync(cancellationToken);
        return new LookupDto(areas, categories);
    }
}
