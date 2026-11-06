namespace FoodBridge.Application.Listings;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListingSummaryDto(
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

public sealed record ListingDetailsDto(
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

public sealed record LookupItemDto(int Id, string Name, string? District);

public sealed record LookupDto(
    IReadOnlyList<LookupItemDto> Areas,
    IReadOnlyList<LookupItemDto> Categories);

public sealed record CloseListingResultDto(int ListingId, string Status, int NoShowCount);
