namespace FoodBridge.Application.Security;

public interface ICurrentUser
{
    string? UserId { get; }

    bool IsAuthenticated { get; }
}
