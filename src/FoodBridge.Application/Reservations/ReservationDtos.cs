using System.ComponentModel.DataAnnotations;

namespace FoodBridge.Application.Reservations;

public sealed class CreateReservationRequest
{
    [Range(1, 10000)]
    public int Quantity { get; set; }
}

public sealed class CollectReservationRequest
{
    [Required]
    public string PickupCode { get; set; } = string.Empty;
}

public sealed record ReservationDto(
    int Id,
    int ListingId,
    int Quantity,
    string PickupCode,
    string Status);
