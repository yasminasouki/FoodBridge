namespace FoodBridge.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "FoodBridge";

    public string Audience { get; set; } = "FoodBridge";

    public string Key { get; set; } = string.Empty;

    public int ExpiryHours { get; set; } = 2;
}
