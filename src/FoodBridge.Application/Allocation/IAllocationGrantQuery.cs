namespace FoodBridge.Application.Allocation;

public interface IAllocationGrantQuery
{
    Task<IReadOnlyList<AllocationGrantDto>> ListAsync(int listingId, CancellationToken cancellationToken = default);
}
