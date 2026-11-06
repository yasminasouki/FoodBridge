using FoodBridge.Application.Listings;
using FoodBridge.Domain.Entities;

namespace FoodBridge.Application.Persistence;

public interface IListingRepository : IRepository<Listing>
{
    Task<PagedResult<ListingSummaryDto>> SearchAsync(
        ListingSearch search,
        CancellationToken cancellationToken = default);

    Task<ListingDetailsDto?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> AllCategoriesExistAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken = default);

    Task AddWithCategoriesAsync(
        Listing listing,
        IReadOnlyCollection<int> categoryIds,
        CancellationToken cancellationToken = default);
}
