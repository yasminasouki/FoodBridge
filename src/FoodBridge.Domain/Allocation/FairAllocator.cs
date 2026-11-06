namespace FoodBridge.Domain.Allocation;

public sealed class FairAllocator : IAllocationStrategy
{
    public IReadOnlyList<AllocationLine> Allocate(
        int availableQuantity,
        IReadOnlyList<AllocationRequest> requests)
    {
        if (availableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity));
        }

        var remaining = availableQuantity;
        var lines = new List<AllocationLine>(requests.Count);

        foreach (var request in requests
            .OrderByDescending(request => request.PriorityScore)
            .ThenBy(request => request.RequestId))
        {
            if (request.RequestedQuantity < 0 || request.MaxUnits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requests));
            }

            var granted = Math.Min(
                request.RequestedQuantity,
                Math.Min(request.MaxUnits, remaining));
            lines.Add(new AllocationLine(request.RequestId, granted));
            remaining -= granted;
        }

        return lines;
    }
}
