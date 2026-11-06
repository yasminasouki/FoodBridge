using FoodBridge.Domain.Enums;

namespace FoodBridge.Application.Allocation;

public sealed class ReservationPolicySelector
{
    private readonly IReservationPolicy _individual;
    private readonly IReservationPolicy _ngo;

    public ReservationPolicySelector(IndividualReservationPolicy individual, NgoReservationPolicy ngo)
    {
        _individual = individual;
        _ngo = ngo;
    }

    public IReservationPolicy For(ReceiverType type) =>
        type == ReceiverType.Ngo ? _ngo : _individual;
}
