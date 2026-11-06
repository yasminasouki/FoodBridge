using FoodBridge.Domain.Common;
using FoodBridge.Domain.Enums;

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
}
