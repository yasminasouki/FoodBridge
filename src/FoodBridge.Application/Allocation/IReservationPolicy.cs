namespace FoodBridge.Application.Allocation;

public interface IReservationPolicy
{
    int MaxUnitsPerRequest { get; }
}
