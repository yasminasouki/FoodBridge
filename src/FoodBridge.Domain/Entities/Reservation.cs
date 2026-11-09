using FoodBridge.Domain.Common;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Domain.Entities;

public sealed class Reservation : AuditableEntity
{
    public int ListingId { get; private set; }
    public string ReceiverId { get; private set; } = string.Empty;
    public int? ClaimRequestId { get; private set; }
    public int Quantity { get; private set; }
    public string PickupCode { get; private set; } = string.Empty;
    public ReservationSource Source { get; private set; } = ReservationSource.Direct;
    public ReservationStatus Status { get; private set; } = ReservationStatus.Pending;
    public DateTime? CollectedAt { get; private set; }

    private Reservation()
    {
    }

    public Reservation(
        int listingId,
        string receiverId,
        int? claimRequestId,
        int quantity,
        string pickupCode,
        ReservationSource source = ReservationSource.Direct)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        if (string.IsNullOrWhiteSpace(receiverId))
        {
            throw new ArgumentException("Receiver is required.", nameof(receiverId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (string.IsNullOrWhiteSpace(pickupCode))
        {
            throw new ArgumentException("Pickup code is required.", nameof(pickupCode));
        }

        ListingId = listingId;
        ReceiverId = receiverId;
        ClaimRequestId = claimRequestId;
        Quantity = quantity;
        PickupCode = pickupCode.Trim();
        Source = source;
    }

    public void Collect(string pickupCode, DateTime utcNow)
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new ConflictException("Only a pending reservation can be collected.");
        }

        if (!string.Equals(PickupCode, pickupCode?.Trim(), StringComparison.Ordinal))
        {
            throw new DomainException("The pickup code is wrong.");
        }

        Status = ReservationStatus.Collected;
        CollectedAt = utcNow;
    }

    public void Cancel()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new ConflictException("Only a pending reservation can be cancelled.");
        }

        Status = ReservationStatus.Cancelled;
    }

    public void MarkNoShow()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new ConflictException("Only a pending reservation can be a no-show.");
        }

        Status = ReservationStatus.NoShow;
    }
}
