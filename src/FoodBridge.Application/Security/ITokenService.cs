namespace FoodBridge.Application.Security;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(
        string userId,
        string email,
        string fullName,
        IReadOnlyList<string> roles);
}
