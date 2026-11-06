namespace FoodBridge.Domain.Exceptions;

public class UserBlockedException : ConflictException
{
    public UserBlockedException(string message) : base(message)
    {
    }
}
