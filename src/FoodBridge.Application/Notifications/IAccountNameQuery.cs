namespace FoodBridge.Application.Notifications;

public interface IAccountNameQuery
{
    Task<string> GetFullNameAsync(string userId, CancellationToken cancellationToken = default);
}
