namespace FoodBridge.Application.Reservations;

public interface IReservationService
{
    Task<ReservationDto> ReserveLeftoverAsync(
        string userId,
        int listingId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<ReservationDto> CollectAsync(
        string userId,
        string pickupCode,
        CancellationToken cancellationToken = default);

    Task CancelAsync(string userId, int reservationId, CancellationToken cancellationToken = default);
}
