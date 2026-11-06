namespace FoodBridge.Application.Activity;

public sealed record MyRequestDto(
    int Id,
    int ListingId,
    string ListingTitle,
    int RequestedQuantity,
    int AllocatedQuantity,
    string Status);

public sealed record MyReservationDto(
    int Id,
    int ListingId,
    string ListingTitle,
    int Quantity,
    string PickupCode,
    string Status,
    DateTime? CollectedAt);

public sealed record ActivityDto(
    int? ReliabilityScore,
    DateTime? BlockedUntil,
    string? ReceiverType,
    IReadOnlyList<MyRequestDto> Requests,
    IReadOnlyList<MyReservationDto> Reservations);
