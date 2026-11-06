using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ClaimRequestRepository : Repository<ClaimRequest>, IClaimRequestRepository
{
    public ClaimRequestRepository(AppDbContext context)
        : base(context)
    {
    }
}
