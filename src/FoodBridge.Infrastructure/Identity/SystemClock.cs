using FoodBridge.Application.Security;

namespace FoodBridge.Infrastructure.Identity;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
