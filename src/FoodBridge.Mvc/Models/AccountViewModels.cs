using System.ComponentModel.DataAnnotations;
using FoodBridge.Mvc.Api;

namespace FoodBridge.Mvc.Models;

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public sealed class RegisterViewModel : IValidatableObject
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Receiver";

    [Range(1, int.MaxValue, ErrorMessage = "Choose an area.")]
    [Display(Name = "Area")]
    public int AreaId { get; set; }

    [MaxLength(150)]
    [Display(Name = "Shop name")]
    public string? ShopName { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [Display(Name = "People served")]
    public int? PeopleServed { get; set; }

    public IReadOnlyList<LookupItem> Areas { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(Role, "Shop", StringComparison.OrdinalIgnoreCase))
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

        if (string.Equals(Role, "Ngo", StringComparison.OrdinalIgnoreCase) && PeopleServed is null or < 1)
        {
            yield return new ValidationResult("People served is required for an NGO.", [nameof(PeopleServed)]);
        }
    }
}

public sealed class QuantityForm
{
    [Range(1, 10000)]
    public int Quantity { get; set; } = 1;
}

public sealed class BrowseViewModel
{
    public Lookups Lookups { get; set; } = new([], []);

    public ListingPage Listings { get; set; } = new([], 1, 12, 0);

    public int? AreaId { get; set; }

    public int? CategoryId { get; set; }

    public int Page { get; set; } = 1;

    public string? Error { get; set; }
}
