namespace FoodBridge.Application.Notifications;

public sealed record NotificationDto(
    int Id,
    string Message,
    int? ListingId,
    DateTime CreatedAt,
    bool IsRead);
