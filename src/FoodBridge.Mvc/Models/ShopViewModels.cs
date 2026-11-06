using System.ComponentModel.DataAnnotations;
using FoodBridge.Mvc.Api;

namespace FoodBridge.Mvc.Models;

public sealed class CreateListingViewModel : IValidatableObject
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int Quantity { get; set; } = 1;

    [Range(0.001, 1000)]
    [Display(Name = "Weight per unit (kg)")]
    public decimal WeightPerUnitKg { get; set; } = 0.4m;

    [Required]
    [Display(Name = "Request window ends")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime RequestWindowEndsAt { get; set; }

    [Required]
    [Display(Name = "Pickup starts")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime PickupFrom { get; set; }

    [Required]
    [Display(Name = "Pickup ends")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime PickupUntil { get; set; }

    [MinLength(1, ErrorMessage = "Choose at least one category.")]
    public int[] CategoryIds { get; set; } = [];

    public IReadOnlyList<LookupItem> Categories { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PickupFrom < RequestWindowEndsAt)
        {
            yield return new ValidationResult("Pickup starts after the request window.", [nameof(PickupFrom)]);
        }

        if (PickupUntil <= PickupFrom)
        {
            yield return new ValidationResult("Pickup ends after it starts.", [nameof(PickupUntil)]);
        }
    }
}

public sealed class CollectForm
{
    [Required]
    [Display(Name = "Pickup code")]
    public string PickupCode { get; set; } = string.Empty;
}

public sealed class ImpactQuery
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime From { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime To { get; set; }

    public int? AreaId { get; set; }

    public IReadOnlyList<LookupItem> Areas { get; set; } = [];

    public IReadOnlyList<ImpactRow> Rows { get; set; } = [];
}
