using FoodBridge.Application.Auth;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;
using FoodBridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Identity;

public sealed class ApprovalService : IApprovalService
{
    private readonly AppDbContext _context;

    public ApprovalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PendingApprovalResponse>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var shops = await _context.Shops
            .AsNoTracking()
            .Where(shop => !shop.IsApproved)
            .Join(
                _context.Users.AsNoTracking(),
                shop => shop.OwnerUserId,
                user => user.Id,
                (shop, user) => new PendingApprovalResponse(
                    user.Id,
                    user.FullName,
                    user.Email ?? string.Empty,
                    "Shop",
                    shop.Name))
            .ToListAsync(cancellationToken);

        var ngos = await _context.ReceiverProfiles
            .AsNoTracking()
            .Where(profile => profile.ReceiverType == ReceiverType.Ngo && !profile.IsApproved)
            .Join(
                _context.Users.AsNoTracking(),
                profile => profile.UserId,
                user => user.Id,
                (profile, user) => new PendingApprovalResponse(
                    user.Id,
                    user.FullName,
                    user.Email ?? string.Empty,
                    "Ngo",
                    user.FullName))
            .ToListAsync(cancellationToken);

        return shops.Concat(ngos).OrderBy(item => item.Email).ToList();
    }

    public async Task ApproveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var shop = await _context.Shops.FirstOrDefaultAsync(
            item => item.OwnerUserId == userId && !item.IsApproved,
            cancellationToken);
        var ngo = await _context.ReceiverProfiles.FirstOrDefaultAsync(
            item => item.UserId == userId && item.ReceiverType == ReceiverType.Ngo && !item.IsApproved,
            cancellationToken);

        if (shop is null && ngo is null)
        {
            throw new NotFoundException("No shop or NGO is waiting for approval for this user.");
        }

        shop?.Approve();
        ngo?.Approve();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
