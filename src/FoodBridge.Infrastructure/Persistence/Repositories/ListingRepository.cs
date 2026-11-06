using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ListingRepository : Repository<Listing>, IListingRepository
{
    public ListingRepository(AppDbContext context)
        : base(context)
    {
    }
}
