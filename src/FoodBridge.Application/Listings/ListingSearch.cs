namespace FoodBridge.Application.Listings;

public sealed record ListingSearch(
    int? AreaId,
    IReadOnlyCollection<int>? CategoryIds,
    bool Mine,
    int? ShopId,
    DateTime UtcNow,
    int Page,
    int PageSize);
