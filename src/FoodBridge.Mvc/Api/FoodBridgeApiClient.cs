using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace FoodBridge.Mvc.Api;

public sealed class FoodBridgeApiClient : IFoodBridgeApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public FoodBridgeApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password },
            Json,
            cancellationToken);
        return await ReadAsync<LoginResult>(response, cancellationToken);
    }

    public async Task RegisterAsync(RegisterAccount account, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/register", account, Json, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public Task<Lookups> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<Lookups>("/api/lookups", cancellationToken);
    }

    public Task<ListingPage> SearchListingsAsync(
        int? areaId,
        int? categoryId,
        int page,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = "12"
        };
        if (areaId is int area)
        {
            query["areaId"] = area.ToString();
        }

        if (categoryId is int category)
        {
            query["categoryIds"] = category.ToString();
        }

        return GetAsync<ListingPage>(QueryHelpers.AddQueryString("/api/listings", query), cancellationToken);
    }

    public Task<ListingPage> GetMyListingsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<ListingPage>("/api/listings?mine=true&pageSize=50", cancellationToken);
    }

    public async Task<ListingDetails> CreateListingAsync(NewListing listing, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/listings", listing, Json, cancellationToken);
        return await ReadAsync<ListingDetails>(response, cancellationToken);
    }

    public async Task<AllocationResult> AllocateAsync(int listingId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync($"/api/listings/{listingId}/allocate", null, cancellationToken);
        return await ReadAsync<AllocationResult>(response, cancellationToken);
    }

    public async Task<CloseResult> CloseListingAsync(int listingId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync($"/api/listings/{listingId}/close", null, cancellationToken);
        return await ReadAsync<CloseResult>(response, cancellationToken);
    }

    public async Task CollectAsync(string pickupCode, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "/api/reservations/collect",
            new { pickupCode },
            Json,
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public Task<IReadOnlyList<ImpactRow>> GetImpactAsync(
        DateTime from,
        DateTime to,
        int? areaId,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["from"] = from.ToString("o"),
            ["to"] = to.ToString("o")
        };
        if (areaId is int area)
        {
            query["areaId"] = area.ToString();
        }

        return GetAsync<IReadOnlyList<ImpactRow>>(QueryHelpers.AddQueryString("/api/reports/impact", query), cancellationToken);
    }

    public Task<IReadOnlyList<PendingApproval>> GetApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<PendingApproval>>("/api/admin/approvals", cancellationToken);
    }

    public async Task ApproveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PatchAsync($"/api/admin/approvals/{Uri.EscapeDataString(userId)}", null, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public Task<ListingDetails> GetListingAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetAsync<ListingDetails>($"/api/listings/{id}", cancellationToken);
    }

    public async Task RequestAsync(int listingId, int quantity, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"/api/listings/{listingId}/requests",
            new { quantity },
            Json,
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task ReserveAsync(int listingId, int quantity, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"/api/listings/{listingId}/reservations",
            new { quantity },
            Json,
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public Task<Activity> GetActivityAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<Activity>("/api/me/activity", cancellationToken);
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        var response = await _http.GetAsync(url, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken))!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await ApiException.FromAsync(response);
        }
    }
}
