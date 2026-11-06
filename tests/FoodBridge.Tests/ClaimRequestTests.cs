using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Tests;

public class ClaimRequestTests
{
    [Fact]
    public void ApplyAllocation_MarksAFullGrantAllocated()
    {
        var claim = new ClaimRequest(1, "receiver", 4);

        claim.ApplyAllocation(grantedQuantity: 4, priorityScore: 86);

        Assert.Equal(ClaimRequestStatus.Allocated, claim.Status);
        Assert.Equal(4, claim.AllocatedQuantity);
        Assert.Equal(86, claim.PriorityScore);
    }

    [Fact]
    public void ApplyAllocation_MarksASmallerGrantPartial()
    {
        var claim = new ClaimRequest(1, "receiver", 4);

        claim.ApplyAllocation(grantedQuantity: 2, priorityScore: 10);

        Assert.Equal(ClaimRequestStatus.Partial, claim.Status);
        Assert.Equal(2, claim.AllocatedQuantity);
    }

    [Fact]
    public void ApplyAllocation_MarksZeroAsNotAllocated()
    {
        var claim = new ClaimRequest(1, "receiver", 4);

        claim.ApplyAllocation(grantedQuantity: 0, priorityScore: 10);

        Assert.Equal(ClaimRequestStatus.NotAllocated, claim.Status);
    }

    [Fact]
    public void ApplyAllocation_RejectsASecondAllocation()
    {
        var claim = new ClaimRequest(1, "receiver", 4);
        claim.ApplyAllocation(1, 10);

        Assert.Throws<ConflictException>(() => claim.ApplyAllocation(1, 10));
    }
}
