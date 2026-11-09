using FoodBridge.Application.Allocation;
using FoodBridge.Application.Notifications;
using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Application.Waitlist;

public sealed class WaitlistService : IWaitlistService
{
    private readonly IListingRepository _listings;
    private readonly IWaitlistRepository _waitlist;
    private readonly IReservationRepository _reservations;
    private readonly IReceiverProfileRepository _profiles;
    private readonly IShopRepository _shops;
    private readonly IUserNotifier _notifier;
    private readonly IAccountNameQuery _names;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ReservationPolicySelector _policies;

    public WaitlistService(
        IListingRepository listings,
        IWaitlistRepository waitlist,
        IReservationRepository reservations,
        IReceiverProfileRepository profiles,
        IShopRepository shops,
        IUserNotifier notifier,
        IAccountNameQuery names,
        IUnitOfWork unitOfWork,
        IClock clock,
        ReservationPolicySelector policies)
    {
        _listings = listings;
        _waitlist = waitlist;
        _reservations = reservations;
        _profiles = profiles;
        _shops = shops;
        _notifier = notifier;
        _names = names;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _policies = policies;
    }

    public async Task JoinAsync(string userId, int listingId, int quantity, CancellationToken cancellationToken = default)
    {
        var profile = await RequireApprovedReceiverAsync(userId, cancellationToken);
        var maxUnits = _policies.For(profile.ReceiverType).MaxUnitsPerRequest;
        if (quantity > maxUnits)
        {
            throw new DomainException($"You can ask for at most {maxUnits} units.");
        }

        var listing = await _listings.GetByIdAsync(listingId, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.Status != ListingStatus.Allocated || listing.RemainingQuantity > 0)
        {
            throw new ConflictException("The waitlist is only for a listing that is already sold out.");
        }

        if (_clock.UtcNow >= listing.PickupUntil)
        {
            throw new ConflictException("The pickup window has ended.");
        }

        if (await _reservations.HasPendingAsync(listingId, userId, cancellationToken))
        {
            throw new ConflictException("You already have food reserved on this listing.");
        }

        var existing = await _waitlist.FindAsync(listingId, userId, cancellationToken);
        if (existing is { Status: WaitlistStatus.Waiting })
        {
            throw new ConflictException("You are already on this waitlist.");
        }

        if (existing is null)
        {
            await _waitlist.AddAsync(new WaitlistEntry(listingId, userId, quantity, _clock.UtcNow), cancellationToken);
        }
        else
        {
            throw new ConflictException("You already joined this waitlist.");
        }

        var name = await _names.GetFullNameAsync(userId, cancellationToken);
        await TellShopAsync(listing, $"{name} joined the waitlist for {listing.Title}.", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LeaveAsync(string userId, int listingId, CancellationToken cancellationToken = default)
    {
        var entry = await _waitlist.FindAsync(listingId, userId, cancellationToken)
            ?? throw new NotFoundException("You are not on this waitlist.");
        var listing = await _listings.GetByIdAsync(listingId, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        entry.Leave();
        var name = await _names.GetFullNameAsync(userId, cancellationToken);
        await TellShopAsync(listing, $"{name} left the waitlist for {listing.Title}.", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task TellShopAsync(Listing listing, string message, CancellationToken cancellationToken)
    {
        var ownerId = await _shops.GetOwnerUserIdAsync(listing.ShopId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ownerId))
        {
            await _notifier.NotifyAsync(ownerId, message, listing.Id, cancellationToken);
        }
    }

    private async Task<ReceiverProfile> RequireApprovedReceiverAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a receiver can join a waitlist.");
        if (!profile.IsApproved)
        {
            throw new ForbiddenException("This account is not approved yet.");
        }

        if (profile.BlockedUntil is DateTime blockedUntil && blockedUntil > _clock.UtcNow)
        {
            throw new UserBlockedException("This account is blocked.");
        }

        return profile;
    }
}
