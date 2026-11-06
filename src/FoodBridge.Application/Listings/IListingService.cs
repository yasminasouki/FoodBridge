namespace FoodBridge.Application.Listings;

public interface IListingService
{
    Task<PagedResult<ListingSummaryDto>> SearchAsync(
        int? areaId,
        int[]? categoryIds,
        bool mine,
        string? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ListingDetailsDto> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<ListingDetailsDto> CreateAsync(
        string userId,
        CreateListingRequest request,
        CancellationToken cancellationToken = default);

    Task CancelAsync(string userId, int id, CancellationToken cancellationToken = default);
}

public interface ILookupService
{
    Task<LookupDto> GetAsync(CancellationToken cancellationToken = default);
}
