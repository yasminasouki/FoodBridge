using FoodBridge.Application.Allocation;
using FoodBridge.Application.Notifications;
using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Application.Reservations;

public sealed class ReservationService : IReservationService
{
    private readonly IListingRepository _listings;
    private readonly IReservationRepository _reservations;
    private readonly IReceiverProfileRepository _profiles;
    private readonly IShopRepository _shops;
    private readonly IWaitlistRepository _waitlist;
    private readonly IUserNotifier _notifier;
    private readonly IAccountNameQuery _names;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ReservationPolicySelector _policies;

    public ReservationService(
        IListingRepository listings,
        IReservationRepository reservations,
        IReceiverProfileRepository profiles,
        IShopRepository shops,
        IWaitlistRepository waitlist,
        IUserNotifier notifier,
        IAccountNameQuery names,
        IUnitOfWork unitOfWork,
        IClock clock,
        ReservationPolicySelector policies)
    {
        _listings = listings;
        _reservations = reservations;
        _profiles = profiles;
        _shops = shops;
        _waitlist = waitlist;
        _notifier = notifier;
        _names = names;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _policies = policies;
    }

    public async Task<ReservationDto> ReserveLeftoverAsync(
        string userId,
        int listingId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a receiver can reserve leftovers.");
        if (!profile.IsApproved)
        {
            throw new ForbiddenException("This account is not approved yet.");
        }

        if (profile.BlockedUntil is DateTime blockedUntil && blockedUntil > _clock.UtcNow)
        {
            throw new UserBlockedException("This account is blocked.");
        }

        var listing = await _listings.GetByIdAsync(listingId, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (_clock.UtcNow >= listing.PickupUntil)
        {
            throw new ConflictException("The pickup window has ended.");
        }

        var maxUnits = _policies.For(profile.ReceiverType).MaxUnitsPerRequest;
        if (quantity > maxUnits)
        {
            throw new DomainException($"You can reserve at most {maxUnits} units.");
        }

        listing.Reserve(quantity);
        var reservation = new Reservation(
            listing.Id,
            userId,
            claimRequestId: null,
            quantity,
            NewPickupCode(),
            ReservationSource.Direct);
        await _reservations.AddAsync(reservation, cancellationToken);
        var ownerId = await _shops.GetOwnerUserIdAsync(listing.ShopId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ownerId))
        {
            var name = await _names.GetFullNameAsync(userId, cancellationToken);
            await _notifier.NotifyAsync(
                ownerId,
                $"{name} reserved {quantity} leftover {(quantity == 1 ? "unit" : "units")} of {listing.Title}.",
                listing.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(reservation);
    }

    public async Task<ReservationDto> CollectAsync(
        string userId,
        string pickupCode,
        CancellationToken cancellationToken = default)
    {
        var shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a shop can confirm a pickup.");
        var code = pickupCode.Trim().ToUpperInvariant();
        var reservation = await _reservations.GetByPickupCodeAsync(code, cancellationToken)
            ?? throw new DomainException("The pickup code is wrong.");
        var listing = await _listings.GetByIdAsync(reservation.ListingId, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.ShopId != shop.Id)
        {
            throw new ForbiddenException("You can only confirm pickups for your own shop.");
        }

        reservation.Collect(code, _clock.UtcNow);
        await _notifier.NotifyAsync(
            reservation.ReceiverId,
            $"{listing.Title}: the shop confirmed your pickup of {reservation.Quantity} {(reservation.Quantity == 1 ? "unit" : "units")}.",
            listing.Id,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(reservation);
    }

    public async Task CancelAsync(string userId, int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException("Reservation was not found.");
        if (reservation.ReceiverId != userId)
        {
            throw new ForbiddenException("You can only cancel your own reservation.");
        }

        var listing = await _listings.GetByIdAsync(reservation.ListingId, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.Status != ListingStatus.Allocated)
        {
            throw new ConflictException("A reservation can be cancelled only while the listing is allocated.");
        }

        reservation.Cancel();
        listing.Release(reservation.Quantity);
        var ownerId = await _shops.GetOwnerUserIdAsync(listing.ShopId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ownerId))
        {
            var name = await _names.GetFullNameAsync(userId, cancellationToken);
            await _notifier.NotifyAsync(
                ownerId,
                $"{name} cancelled {reservation.Quantity} {(reservation.Quantity == 1 ? "unit" : "units")} of {listing.Title}.",
                listing.Id,
                cancellationToken);
        }

        await OfferReleasedFoodAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task OfferReleasedFoodAsync(Listing listing, CancellationToken cancellationToken)
    {
        var waiting = await _waitlist.ListWaitingAsync(listing.Id, cancellationToken);
        if (waiting.Count == 0 || listing.RemainingQuantity == 0)
        {
            return;
        }

        var profiles = await _profiles.GetTrackedByUserIdsAsync(
            waiting.Select(entry => entry.ReceiverId).Distinct().ToArray(),
            cancellationToken);
        var profilesByUser = profiles.ToDictionary(profile => profile.UserId);
        foreach (var entry in waiting)
        {
            if (listing.RemainingQuantity == 0)
            {
                break;
            }

            if (!profilesByUser.TryGetValue(entry.ReceiverId, out var profile) || !profile.IsApproved)
            {
                continue;
            }

            if (profile.BlockedUntil is DateTime blockedUntil && blockedUntil > _clock.UtcNow)
            {
                continue;
            }

            var maxUnits = _policies.For(profile.ReceiverType).MaxUnitsPerRequest;
            var grant = Math.Min(entry.RequestedQuantity, Math.Min(maxUnits, listing.RemainingQuantity));
            if (grant <= 0)
            {
                continue;
            }

            listing.Reserve(grant);
            var pickupCode = NewPickupCode();
            await _reservations.AddAsync(
                new Reservation(listing.Id, entry.ReceiverId, claimRequestId: null, grant, pickupCode, ReservationSource.Waitlist),
                cancellationToken);
            entry.Promote();
            await _notifier.NotifyAsync(
                entry.ReceiverId,
                $"{listing.Title}: {grant} {(grant == 1 ? "unit is" : "units are")} ready. Pickup code {pickupCode}. If you miss this waitlist offer, your reliability score does not change.",
                listing.Id,
                cancellationToken);
        }
    }

    private static ReservationDto ToDto(Reservation reservation) =>
        new(reservation.Id, reservation.ListingId, reservation.Quantity, reservation.PickupCode, reservation.Status.ToString());

    private static string NewPickupCode() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
