using FoodBridge.Application.Allocation;
using FoodBridge.Domain.Enums;

namespace FoodBridge.Tests;

public class ReservationPolicyTests
{
    private readonly ReservationPolicySelector _selector = new(
        new IndividualReservationPolicy(),
        new NgoReservationPolicy());

    [Fact]
    public void Selector_GivesAnNgoTheHigherLimit()
    {
        Assert.Equal(10, _selector.For(ReceiverType.Ngo).MaxUnitsPerRequest);
    }

    [Fact]
    public void Selector_GivesAnIndividualTheLowerLimit()
    {
        Assert.Equal(2, _selector.For(ReceiverType.Individual).MaxUnitsPerRequest);
    }
}
