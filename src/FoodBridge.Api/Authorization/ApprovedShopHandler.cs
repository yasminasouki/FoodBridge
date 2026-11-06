using FoodBridge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Api.Authorization;

public sealed class ApprovedShopHandler : AuthorizationHandler<ApprovedShopRequirement>
{
    private readonly AppDbContext _context;

    public ApprovedShopHandler(AppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApprovedShopRequirement requirement)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        if (userId is null)
        {
            return;
        }

        var approved = await _context.Shops
            .AsNoTracking()
            .AnyAsync(shop => shop.OwnerUserId == userId && shop.IsApproved);

        if (approved)
        {
            context.Succeed(requirement);
        }
    }
}
