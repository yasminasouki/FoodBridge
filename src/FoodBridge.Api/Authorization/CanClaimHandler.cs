using FoodBridge.Application.Security;
using FoodBridge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Api.Authorization;

public sealed class CanClaimHandler : AuthorizationHandler<CanClaimRequirement>
{
    private readonly AppDbContext _context;
    private readonly IClock _clock;

    public CanClaimHandler(AppDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanClaimRequirement requirement)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        if (userId is null)
        {
            return;
        }

        var profile = await _context.ReceiverProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == userId);

        var blocked = profile?.BlockedUntil is not null && profile.BlockedUntil > _clock.UtcNow;
        if (profile is { IsApproved: true } && !blocked)
        {
            context.Succeed(requirement);
        }
    }
}
