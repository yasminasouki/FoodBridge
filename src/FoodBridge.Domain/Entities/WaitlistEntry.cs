using FoodBridge.Domain.Common;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Domain.Entities;

public sealed class WaitlistEntry : BaseEntity
{
    public int ListingId { get; private set; }
    public string ReceiverId { get; private set; } = string.Empty;
    public int RequestedQuantity { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public WaitlistStatus Status { get; private set; } = WaitlistStatus.Waiting;

    private WaitlistEntry()
    {
    }

    public WaitlistEntry(int listingId, string receiverId, int requestedQuantity, DateTime joinedAt)
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
        JoinedAt = joinedAt;
    }

    public void Promote()
    {
        if (Status != WaitlistStatus.Waiting)
        {
            throw new ConflictException("Only a waiting entry can be offered food.");
        }

        Status = WaitlistStatus.Promoted;
    }

    public void Leave()
    {
        if (Status != WaitlistStatus.Waiting)
        {
            throw new ConflictException("Only a waiting entry can leave the waitlist.");
        }

        Status = WaitlistStatus.Left;
    }
}
