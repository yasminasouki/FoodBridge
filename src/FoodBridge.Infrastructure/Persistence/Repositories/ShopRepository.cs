using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ShopRepository : IShopRepository
{
    private readonly AppDbContext _context;

    public ShopRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Shop?> GetByOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default)
    {
        return _context.Shops.FirstOrDefaultAsync(shop => shop.OwnerUserId == ownerUserId, cancellationToken);
    }
}
