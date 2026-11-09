namespace FoodBridge.Application.Allocation;

public sealed record AllocationGrantDto(
    int RequestId,
    string ReceiverName,
    int RequestedQuantity,
    int GrantedQuantity,
    int PriorityScore,
    string Status,
    string? PickupCode);
