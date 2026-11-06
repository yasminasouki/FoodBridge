using FoodBridge.Application.Persistence;

namespace FoodBridge.Application.Activity;

public sealed class ActivityService : IActivityService
{
    private readonly IActivityQuery _activity;
    private readonly IReceiverProfileRepository _profiles;

    public ActivityService(IActivityQuery activity, IReceiverProfileRepository profiles)
    {
        _activity = activity;
        _profiles = profiles;
    }

    public async Task<ActivityDto> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        var profile = await _profiles.GetByUserIdAsync(userId, cancellationToken);
        var requests = await _activity.ListRequestsAsync(userId, cancellationToken);
        var reservations = await _activity.ListReservationsAsync(userId, cancellationToken);
        return new ActivityDto(
            profile?.ReliabilityScore,
            profile?.BlockedUntil,
            profile?.ReceiverType.ToString(),
            requests,
            reservations);
    }
}
