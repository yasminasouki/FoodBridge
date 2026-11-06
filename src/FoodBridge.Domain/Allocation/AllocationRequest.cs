namespace FoodBridge.Domain.Allocation;

public sealed record AllocationRequest(
    int RequestId,
    int RequestedQuantity,
    int PriorityScore,
    int MaxUnits);
