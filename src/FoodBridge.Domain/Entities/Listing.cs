using FoodBridge.Domain.Common;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Domain.Entities;

public sealed class Listing : AuditableEntity
{
    public int ShopId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int OriginalQuantity { get; private set; }
    public int RemainingQuantity { get; private set; }
    public decimal WeightPerUnitKg { get; private set; }
    public DateTime RequestWindowEndsAt { get; private set; }
    public DateTime PickupFrom { get; private set; }
    public DateTime PickupUntil { get; private set; }
    public ListingStatus Status { get; private set; } = ListingStatus.Open;
    public byte[] RowVersion { get; private set; } = [];

    private Listing()
    {
    }

    public Listing(
        int shopId,
        string title,
        int originalQuantity,
        decimal weightPerUnitKg,
        DateTime requestWindowEndsAt,
        DateTime pickupFrom,
        DateTime pickupUntil)
    {
        if (shopId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shopId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (originalQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originalQuantity));
        }

        if (weightPerUnitKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightPerUnitKg));
        }

        if (pickupFrom < requestWindowEndsAt)
        {
            throw new ArgumentException("Pickup starts after the request window.", nameof(pickupFrom));
        }

        if (pickupUntil <= pickupFrom)
        {
            throw new ArgumentException("Pickup ends after it starts.", nameof(pickupUntil));
        }

        ShopId = shopId;
        Title = title.Trim();
        OriginalQuantity = originalQuantity;
        RemainingQuantity = originalQuantity;
        WeightPerUnitKg = weightPerUnitKg;
        RequestWindowEndsAt = requestWindowEndsAt;
        PickupFrom = pickupFrom;
        PickupUntil = pickupUntil;
    }

    public void MarkAllocated()
    {
        if (Status != ListingStatus.Open)
        {
            throw new ConflictException("Only an open listing can be allocated.");
        }

        Status = ListingStatus.Allocated;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ConflictException("Quantity must be at least 1.");
        }

        if (Status != ListingStatus.Allocated)
        {
            throw new ConflictException("Leftovers can be reserved only after allocation.");
        }

        if (quantity > RemainingQuantity)
        {
            throw new ConflictException("Not enough food left.");
        }

        RemainingQuantity -= quantity;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ConflictException("Quantity must be at least 1.");
        }

        if (RemainingQuantity + quantity > OriginalQuantity)
        {
            throw new ConflictException("Release would exceed the original quantity.");
        }

        RemainingQuantity += quantity;
    }

    public void Cancel()
    {
        if (Status != ListingStatus.Open)
        {
            throw new ConflictException("Only an open listing can be cancelled.");
        }

        Status = ListingStatus.Cancelled;
    }

    public void Close(DateTime utcNow)
    {
        if (Status is ListingStatus.Closed or ListingStatus.Cancelled)
        {
            throw new ConflictException("This listing is already finished.");
        }

        if (utcNow < PickupUntil)
        {
            throw new ConflictException("A listing can be closed only after the pickup window has ended.");
        }

        Status = ListingStatus.Closed;
    }
}
