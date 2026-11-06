namespace FoodBridge.Application.Allocation;

public sealed class IndividualReservationPolicy : ReservationPolicyBase
{
    public override int MaxUnitsPerRequest => 2;
}
