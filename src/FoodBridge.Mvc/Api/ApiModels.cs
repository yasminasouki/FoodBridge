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
