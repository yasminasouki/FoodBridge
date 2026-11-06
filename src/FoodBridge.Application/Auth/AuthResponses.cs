namespace FoodBridge.Application.Auth;

public sealed record RegisterResponse(string UserId, string Email, string Role);

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc);

public sealed record PendingApprovalResponse(
    string UserId,
    string FullName,
    string Email,
    string Kind,
    string Name);
