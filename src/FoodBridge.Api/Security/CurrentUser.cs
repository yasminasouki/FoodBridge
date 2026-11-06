using System.Security.Claims;
using FoodBridge.Application.Security;

namespace FoodBridge.Api.Security;

public sealed class CurrentUser : ICurrentUser
{
    public CurrentUser(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        UserId = user?.FindFirstValue("sub");
        IsAuthenticated = user?.Identity?.IsAuthenticated == true;
    }

    public string? UserId { get; }

    public bool IsAuthenticated { get; }
}
