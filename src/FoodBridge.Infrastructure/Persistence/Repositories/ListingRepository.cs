using FoodBridge.Application.Listings;
using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ListingRepository : Repository<Listing>, IListingRepository
{
    private readonly AppDbContext _context;

    public ListingRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<ListingSummaryDto>> SearchAsync(
        ListingSearch search,
        CancellationToken cancellationToken = default)
    {
        var query = Filtered(search);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(listing => listing.RequestWindowEndsAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(listing => new ListingSummaryDto(
                listing.Id,
                listing.Title,
                _context.Shops.Where(shop => shop.Id == listing.ShopId).Select(shop => shop.Name).First(),
                _context.Shops.Where(shop => shop.Id == listing.ShopId)
                    .Join(_context.Areas, shop => shop.AreaId, area => area.Id, (shop, area) => area.Name)
                    .First(),
                listing.RemainingQuantity,
                listing.WeightPerUnitKg,
                listing.RequestWindowEndsAt,
                listing.PickupFrom,
                listing.PickupUntil,
                listing.Status.ToString(),
                _context.ListingCategories
                    .Where(link => link.ListingId == listing.Id)
                    .Join(_context.Categories, link => link.CategoryId, category => category.Id, (link, category) => category.Name)
                    .ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<ListingSummaryDto>(items, search.Page, search.PageSize, total);
    }

    public Task<ListingDetailsDto?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Listings
            .AsNoTracking()
            .Where(listing => listing.Id == id)
            .Select(listing => new ListingDetailsDto(
                listing.Id,
                listing.ShopId,
                listing.Title,
                _context.Shops.Where(shop => shop.Id == listing.ShopId).Select(shop => shop.Name).First(),
                _context.Shops.Where(shop => shop.Id == listing.ShopId)
                    .Join(_context.Areas, shop => shop.AreaId, area => area.Id, (shop, area) => area.Name)
                    .First(),
                _context.Shops.Where(shop => shop.Id == listing.ShopId).Select(shop => shop.Address).First(),
                listing.OriginalQuantity,
                listing.RemainingQuantity,
                listing.WeightPerUnitKg,
                listing.RequestWindowEndsAt,
                listing.PickupFrom,
                listing.PickupUntil,
                listing.Status.ToString(),
                _context.ListingCategories
                    .Where(link => link.ListingId == listing.Id)
                    .Join(_context.Categories, link => link.CategoryId, category => category.Id, (link, category) => category.Name)
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> AllCategoriesExistAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken = default)
    {
        var found = await _context.Categories.CountAsync(
            category => categoryIds.Contains(category.Id),
            cancellationToken);
        return found == categoryIds.Count;
    }

    public async Task AddWithCategoriesAsync(
        Listing listing,
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Listings.AddAsync(listing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        foreach (var categoryId in categoryIds)
        {
            _context.ListingCategories.Add(new ListingCategory(listing.Id, categoryId));
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private IQueryable<Listing> Filtered(ListingSearch search)
    {
        var query = _context.Listings.AsNoTracking();
        if (search.Mine)
        {
            query = query.Where(listing => listing.ShopId == search.ShopId);
        }
        else
        {
            query = query.Where(listing =>
                listing.Status == ListingStatus.Open &&
                listing.RequestWindowEndsAt > search.UtcNow);
        }

        if (search.AreaId is int areaId)
        {
            query = query.Where(listing =>
                _context.Shops.Any(shop => shop.Id == listing.ShopId && shop.AreaId == areaId));
        }

        if (search.CategoryIds is { Count: > 0 })
        {
            var categoryIds = search.CategoryIds;
            query = query.Where(listing =>
                _context.ListingCategories.Any(link =>
                    link.ListingId == listing.Id && categoryIds.Contains(link.CategoryId)));
        }

        return query;
    }
}
