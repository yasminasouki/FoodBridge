using FoodBridge.Application.Allocation;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ReservationPolicySelector _policies;

    public ReservationService(
        IListingRepository listings,
        IReservationRepository reservations,
        IReceiverProfileRepository profiles,
        IShopRepository shops,
        IUnitOfWork unitOfWork,
        IClock clock,
        ReservationPolicySelector policies)
    {
        _listings = listings;
        _reservations = reservations;
        _profiles = profiles;
        _shops = shops;
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
        var reservation = new Reservation(listing.Id, userId, claimRequestId: null, quantity, NewPickupCode());
        await _reservations.AddAsync(reservation, cancellationToken);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ReservationDto ToDto(Reservation reservation) =>
        new(reservation.Id, reservation.ListingId, reservation.Quantity, reservation.PickupCode, reservation.Status.ToString());

    private static string NewPickupCode() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
