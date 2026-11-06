namespace FoodBridge.Application.Security;

public interface IClock
{
    DateTime UtcNow { get; }
}
