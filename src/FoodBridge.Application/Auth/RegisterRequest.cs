using System.ComponentModel.DataAnnotations;
using FoodBridge.Application.Security;

namespace FoodBridge.Application.Auth;

public sealed class RegisterRequest : IValidatableObject
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int AreaId { get; set; }

    [MaxLength(150)]
    public string? ShopName { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    public int? PeopleServed { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Is(Roles.Shop))
        {
            if (string.IsNullOrWhiteSpace(ShopName))
            {
                yield return new ValidationResult("Shop name is required.", [nameof(ShopName)]);
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                yield return new ValidationResult("Address is required.", [nameof(Address)]);
            }

            yield break;
        }

        if (Is(Roles.Ngo))
        {
            if (PeopleServed is null or < 1)
            {
                yield return new ValidationResult("People served is required for an NGO.", [nameof(PeopleServed)]);
            }

            yield break;
        }

        if (!Is(Roles.Receiver))
        {
            yield return new ValidationResult("Role must be Shop, Receiver, or Ngo.", [nameof(Role)]);
        }
    }

    private bool Is(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
}
