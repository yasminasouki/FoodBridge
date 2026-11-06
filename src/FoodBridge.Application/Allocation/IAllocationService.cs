namespace FoodBridge.Application.Allocation;

public interface IAllocationService
{
    Task<ClaimRequestDto> RequestAsync(
        string userId,
        int listingId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<AllocationResultDto> AllocateAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default);
}
