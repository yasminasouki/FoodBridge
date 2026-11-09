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

    Task<ListingPage> GetMyListingsAsync(CancellationToken cancellationToken = default);

    Task<ListingDetails> CreateListingAsync(NewListing listing, CancellationToken cancellationToken = default);

    Task<AllocationResult> AllocateAsync(int listingId, CancellationToken cancellationToken = default);

    Task<CloseResult> CloseListingAsync(int listingId, CancellationToken cancellationToken = default);

    Task CancelListingAsync(int listingId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AllocationGrant>> GetAllocationAsync(int listingId, CancellationToken cancellationToken = default);

    Task JoinWaitlistAsync(int listingId, int quantity, CancellationToken cancellationToken = default);

    Task CancelReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notice>> GetNotificationsAsync(CancellationToken cancellationToken = default);

    Task CollectAsync(string pickupCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImpactRow>> GetImpactAsync(
        DateTime from,
        DateTime to,
        int? areaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingApproval>> GetApprovalsAsync(CancellationToken cancellationToken = default);

    Task ApproveAsync(string userId, CancellationToken cancellationToken = default);

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

public sealed class NewListing
{
    public string Title { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal WeightPerUnitKg { get; set; }

    public DateTime RequestWindowEndsAt { get; set; }

    public DateTime PickupFrom { get; set; }

    public DateTime PickupUntil { get; set; }

    public int[] CategoryIds { get; set; } = [];
}
