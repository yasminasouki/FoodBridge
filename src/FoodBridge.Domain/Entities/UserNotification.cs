using FoodBridge.Domain.Common;

namespace FoodBridge.Domain.Entities;

public sealed class UserNotification : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public int? ListingId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private UserNotification()
    {
    }

    public UserNotification(string userId, string message, int? listingId, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message is required.", nameof(message));
        }

        UserId = userId;
        Message = message.Trim();
        ListingId = listingId;
        CreatedAt = createdAt;
    }

    public void MarkRead(DateTime utcNow)
    {
        ReadAt ??= utcNow;
    }
}
