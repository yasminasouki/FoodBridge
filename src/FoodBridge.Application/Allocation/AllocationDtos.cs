using System.ComponentModel.DataAnnotations;

namespace FoodBridge.Application.Allocation;

public sealed class CreateClaimRequest
{
    [Range(1, 10000)]
    public int Quantity { get; set; }
}

public sealed record ClaimRequestDto(
    int Id,
    int ListingId,
    int RequestedQuantity,
    string Status);

public sealed record AllocationLineDto(
    int RequestId,
    string ReceiverId,
    int RequestedQuantity,
    int GrantedQuantity,
    int PriorityScore,
    string Status,
    string? PickupCode);

public sealed record AllocationResultDto(
    int ListingId,
    string Status,
    int RemainingQuantity,
    IReadOnlyList<AllocationLineDto> Lines);
