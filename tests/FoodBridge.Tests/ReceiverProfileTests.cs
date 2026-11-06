using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;

namespace FoodBridge.Tests;

public class ReceiverProfileTests
{
    private static readonly DateTime Now = new(2026, 11, 6, 20, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ApplyNoShow_SubtractsTwenty_AndBlocksOnlyBelowForty()
    {
        var profile = new ReceiverProfile("user-1", ReceiverType.Individual, peopleServed: 1);

        profile.ApplyNoShow(Now);
        Assert.Equal(60, profile.ReliabilityScore);
        Assert.Null(profile.BlockedUntil);

        profile.ApplyNoShow(Now);
        Assert.Equal(40, profile.ReliabilityScore);
        Assert.Null(profile.BlockedUntil);

        profile.ApplyNoShow(Now);
        Assert.Equal(20, profile.ReliabilityScore);
        Assert.Equal(Now.AddDays(3), profile.BlockedUntil);
    }
}
