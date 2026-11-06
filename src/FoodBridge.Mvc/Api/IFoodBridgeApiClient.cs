namespace FoodBridge.Mvc.Api;

public interface IFoodBridgeApiClient
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task RegisterAsync(RegisterAccount account, CancellationToken cancellationToken = default);

    Task<Lookups> GetLookupsAsync(CancellationToken cancellationToken = default);

    Task<ListingPage> SearchListingsAsync(
        int? areaId,
        int? categoryId,
        int page,
        CancellationToken cancellationToken = default);

    Task<ListingDetails> GetListingAsync(int id, CancellationToken cancellationToken = default);

    Task RequestAsync(int listingId, int quantity, CancellationToken cancellationToken = default);

    Task ReserveAsync(int listingId, int quantity, CancellationToken cancellationToken = default);

    Task<Activity> GetActivityAsync(CancellationToken cancellationToken = default);
}

public sealed class RegisterAccount
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int AreaId { get; set; }

    public string? ShopName { get; set; }

    public string? Address { get; set; }

    public int? PeopleServed { get; set; }
}
