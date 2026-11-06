namespace FoodBridge.Application.Activity;

public interface IActivityService
{
    Task<ActivityDto> GetAsync(string userId, CancellationToken cancellationToken = default);
}

public interface IActivityQuery
{
    Task<IReadOnlyList<MyRequestDto>> ListRequestsAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MyReservationDto>> ListReservationsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
