using Microsoft.AspNetCore.Identity;

namespace FoodBridge.Infrastructure.Identity;

public sealed class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public int? AreaId { get; set; }
}
