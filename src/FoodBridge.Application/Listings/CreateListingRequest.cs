using System.ComponentModel.DataAnnotations;

namespace FoodBridge.Application.Listings;

public sealed class CreateListingRequest : IValidatableObject
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int Quantity { get; set; }

    [Range(0.001, 1000)]
    public decimal WeightPerUnitKg { get; set; }

    [Required]
    public DateTime RequestWindowEndsAt { get; set; }

    [Required]
    public DateTime PickupFrom { get; set; }

    [Required]
    public DateTime PickupUntil { get; set; }

    [Required]
    [MinLength(1)]
    public int[] CategoryIds { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PickupFrom < RequestWindowEndsAt)
        {
            yield return new ValidationResult(
                "Pickup starts after the request window.",
                [nameof(PickupFrom)]);
        }

        if (PickupUntil <= PickupFrom)
        {
            yield return new ValidationResult(
                "Pickup ends after it starts.",
                [nameof(PickupUntil)]);
        }
    }
}
