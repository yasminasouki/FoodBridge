using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FoodBridge.Mvc.Security;

public static class AccessToken
{
    public const string ClaimType = "access_token";

    public static ClaimsIdentity ReadIdentity(string token)
    {
        var payload = token.Split('.')[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
        var root = document.RootElement;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, root.GetProperty("sub").GetString() ?? string.Empty),
            new(ClaimTypes.Email, root.GetProperty("email").GetString() ?? string.Empty),
            new(ClaimTypes.Name, root.GetProperty("name").GetString() ?? string.Empty),
            new(ClaimTypes.Role, root.GetProperty("role").GetString() ?? string.Empty),
            new(ClaimType, token)
        };
        return new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
