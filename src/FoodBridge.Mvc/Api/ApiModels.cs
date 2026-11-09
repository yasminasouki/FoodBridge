namespace FoodBridge.Mvc.Api;

public sealed record LoginResult(string Token, DateTime ExpiresAtUtc);

public sealed record LookupItem(int Id, string Name, string? District);

public sealed record Lookups(IReadOnlyList<LookupItem> Areas, IReadOnlyList<LookupItem> Categories);

public sealed record ListingSummary(
    int Id,
    string Title,
    string ShopName,
    string AreaName,
    int RemainingQuantity,
    decimal WeightPerUnitKg,
    DateTime RequestWindowEndsAt,
    DateTime PickupFrom,
    DateTime PickupUntil,
    string Status,
    IReadOnlyList<string> Categories);

public sealed record ListingDetails(
    int Id,
    int ShopId,
    string Title,
    string ShopName,
    string AreaName,
    string Address,
    int OriginalQuantity,
    int RemainingQuantity,
    decimal WeightPerUnitKg,
    DateTime RequestWindowEndsAt,
    DateTime PickupFrom,
    DateTime PickupUntil,
    string Status,
    IReadOnlyList<string> Categories);

public sealed record ListingPage(
    IReadOnlyList<ListingSummary> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record MyRequest(
    int Id,
    int ListingId,
    string ListingTitle,
    int RequestedQuantity,
    int AllocatedQuantity,
    string Status);

public sealed record MyReservation(
    int Id,
    int ListingId,
    string ListingTitle,
    int Quantity,
    string PickupCode,
    string Status,
    DateTime? CollectedAt);

public sealed record Activity(
    int? ReliabilityScore,
    DateTime? BlockedUntil,
    string? ReceiverType,
    IReadOnlyList<MyRequest> Requests,
    IReadOnlyList<MyReservation> Reservations);

public sealed record AllocationLine(
    int RequestId,
    int RequestedQuantity,
    int GrantedQuantity,
    int PriorityScore,
    string Status,
    string? PickupCode);

public sealed record AllocationResult(
    int ListingId,
    string Status,
    int RemainingQuantity,
    IReadOnlyList<AllocationLine> Lines);

public sealed record CloseResult(int ListingId, string Status, int NoShowCount);

public sealed record PendingApproval(
    string UserId,
    string FullName,
    string Email,
    string Kind,
    string Name);

public sealed record AllocationGrant(
    int RequestId,
    string ReceiverName,
    int RequestedQuantity,
    int GrantedQuantity,
    int PriorityScore,
    string Status,
    string? PickupCode);

public sealed record Notice(
    int Id,
    string Message,
    int? ListingId,
    DateTime CreatedAt,
    bool IsRead);

public sealed record ImpactRow(
    int ShopId,
    string ShopName,
    int AreaId,
    string AreaName,
    decimal KilosSaved,
    int CollectedUnits,
    int NoShowUnits,
    decimal CollectionRate,
    decimal NoShowRate);
