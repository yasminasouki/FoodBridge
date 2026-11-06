namespace FoodBridge.Application.Allocation;

public sealed class NgoReservationPolicy : ReservationPolicyBase
{
    public override int MaxUnitsPerRequest => 10;
}
