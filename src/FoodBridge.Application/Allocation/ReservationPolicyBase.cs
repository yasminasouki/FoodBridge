namespace FoodBridge.Application.Allocation;

public abstract class ReservationPolicyBase : IReservationPolicy
{
    public abstract int MaxUnitsPerRequest { get; }
}
