using FoodBridge.Domain.Common;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Domain.Entities;

public sealed class ClaimRequest : BaseEntity
{
    public int ListingId { get; private set; }
    public string ReceiverId { get; private set; } = string.Empty;
    public int RequestedQuantity { get; private set; }
    public int AllocatedQuantity { get; private set; }
    public int PriorityScore { get; private set; }
    public ClaimRequestStatus Status { get; private set; } = ClaimRequestStatus.Pending;

    private ClaimRequest()
    {
    }

    public ClaimRequest(int listingId, string receiverId, int requestedQuantity)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        if (string.IsNullOrWhiteSpace(receiverId))
        {
            throw new ArgumentException("Receiver is required.", nameof(receiverId));
        }

        if (requestedQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedQuantity));
        }

        ListingId = listingId;
        ReceiverId = receiverId;
        RequestedQuantity = requestedQuantity;
    }

    public void ApplyAllocation(int grantedQuantity, int priorityScore)
    {
        if (Status != ClaimRequestStatus.Pending)
        {
            throw new ConflictException("This request was already allocated.");
        }

        if (grantedQuantity < 0 || grantedQuantity > RequestedQuantity)
        {
            throw new ConflictException("The granted quantity is not valid.");
        }

        if (priorityScore < 0)
        {
            throw new ConflictException("The priority score is not valid.");
        }

        AllocatedQuantity = grantedQuantity;
        PriorityScore = priorityScore;
        Status = grantedQuantity == 0
            ? ClaimRequestStatus.NotAllocated
            : grantedQuantity < RequestedQuantity
                ? ClaimRequestStatus.Partial
                : ClaimRequestStatus.Allocated;
    }
}
