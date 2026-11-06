using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Tests;

public class ListingTests
{
    private static readonly DateTime PickupUntil = new(2026, 11, 6, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Reserve_ReducesRemainingQuantity_AfterAllocation()
    {
        var listing = NewListing(originalQuantity: 4);
        listing.MarkAllocated();

        listing.Reserve(1);

        Assert.Equal(3, listing.RemainingQuantity);
        Assert.Equal(ListingStatus.Allocated, listing.Status);
    }

    [Fact]
    public void Reserve_RejectsMoreThanWhatIsLeft()
    {
        var listing = NewListing(originalQuantity: 1);
        listing.MarkAllocated();

        var error = Assert.Throws<ConflictException>(() => listing.Reserve(2));

        Assert.Equal("Not enough food left.", error.Message);
        Assert.Equal(1, listing.RemainingQuantity);
    }

    [Fact]
    public void Reserve_RejectsLeftovers_BeforeAllocation()
    {
        var listing = NewListing(originalQuantity: 2);

        Assert.Throws<ConflictException>(() => listing.Reserve(1));
    }

    [Fact]
    public void Close_RejectsATimeInsideThePickupWindow()
    {
        var listing = NewListing(originalQuantity: 2);

        var error = Assert.Throws<ConflictException>(() => listing.Close(PickupUntil.AddMinutes(-1)));

        Assert.Equal(ListingStatus.Open, listing.Status);
        Assert.Contains("pickup window", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancel_CancelsAnOpenListing()
    {
        var listing = NewListing(originalQuantity: 2);

        listing.Cancel();

        Assert.Equal(ListingStatus.Cancelled, listing.Status);
    }

    [Fact]
    public void Cancel_RejectsAListingThatWasAllocated()
    {
        var listing = NewListing(originalQuantity: 2);
        listing.MarkAllocated();

        Assert.Throws<ConflictException>(listing.Cancel);
    }

    [Fact]
    public void Close_Closes_AfterThePickupWindowEnds()
    {
        var listing = NewListing(originalQuantity: 2);
        listing.MarkAllocated();

        listing.Close(PickupUntil);

        Assert.Equal(ListingStatus.Closed, listing.Status);
    }

    private static Listing NewListing(int originalQuantity)
    {
        return new Listing(
            shopId: 1,
            title: "Bread",
            originalQuantity: originalQuantity,
            weightPerUnitKg: 0.5m,
            requestWindowEndsAt: PickupUntil.AddHours(-4),
            pickupFrom: PickupUntil.AddHours(-2),
            pickupUntil: PickupUntil);
    }
}
