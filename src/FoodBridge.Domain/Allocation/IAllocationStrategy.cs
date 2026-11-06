namespace FoodBridge.Domain.Allocation;

public interface IAllocationStrategy
{
    IReadOnlyList<AllocationLine> Allocate(
        int availableQuantity,
        IReadOnlyList<AllocationRequest> requests);
}
