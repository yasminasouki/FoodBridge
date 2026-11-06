using FoodBridge.Domain.Allocation;

namespace FoodBridge.Tests;

public class FairAllocatorTests
{
    private readonly FairAllocator _allocator = new();

    [Fact]
    public void Allocate_ServesHigherScoreFirst_WhenFoodIsShort()
    {
        var requests = new[]
        {
            new AllocationRequest(1, RequestedQuantity: 3, PriorityScore: 10, MaxUnits: 10),
            new AllocationRequest(2, RequestedQuantity: 3, PriorityScore: 30, MaxUnits: 10),
            new AllocationRequest(3, RequestedQuantity: 3, PriorityScore: 20, MaxUnits: 10)
        };

        var lines = _allocator.Allocate(availableQuantity: 4, requests)
            .ToDictionary(line => line.RequestId);

        Assert.Equal(3, lines[2].GrantedQuantity);
        Assert.Equal(1, lines[3].GrantedQuantity);
        Assert.Equal(0, lines[1].GrantedQuantity);
    }

    [Fact]
    public void Allocate_EarlierRequestWins_WhenScoresMatch()
    {
        var requests = new[]
        {
            new AllocationRequest(8, RequestedQuantity: 2, PriorityScore: 15, MaxUnits: 10),
            new AllocationRequest(3, RequestedQuantity: 2, PriorityScore: 15, MaxUnits: 10)
        };

        var lines = _allocator.Allocate(availableQuantity: 2, requests)
            .ToDictionary(line => line.RequestId);

        Assert.Equal(2, lines[3].GrantedQuantity);
        Assert.Equal(0, lines[8].GrantedQuantity);
    }

    [Fact]
    public void Allocate_NeverExceedsMaxUnits()
    {
        var requests = new[]
        {
            new AllocationRequest(1, RequestedQuantity: 10, PriorityScore: 50, MaxUnits: 2)
        };

        var line = Assert.Single(_allocator.Allocate(availableQuantity: 10, requests));

        Assert.Equal(2, line.GrantedQuantity);
    }

    [Fact]
    public void Allocate_GivesNothing_WhenNoFoodIsLeft()
    {
        var requests = new[]
        {
            new AllocationRequest(1, RequestedQuantity: 1, PriorityScore: 1, MaxUnits: 2)
        };

        var line = Assert.Single(_allocator.Allocate(availableQuantity: 0, requests));

        Assert.Equal(0, line.GrantedQuantity);
    }
}
