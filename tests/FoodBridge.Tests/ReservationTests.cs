using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Tests;

public class ReservationTests
{
    [Fact]
    public void Collect_MarksTheReservationCollected()
    {
        var reservation = NewReservation();
        var at = new DateTime(2026, 11, 6, 18, 0, 0, DateTimeKind.Utc);

        reservation.Collect("CODE1", at);

        Assert.Equal(ReservationStatus.Collected, reservation.Status);
        Assert.Equal(at, reservation.CollectedAt);
    }

    [Fact]
    public void Collect_RejectsTheWrongCode()
    {
        var reservation = NewReservation();

        Assert.Throws<DomainException>(() => reservation.Collect("NOPE", DateTime.UtcNow));
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void Cancel_ReturnsTheReservationToCancelled()
    {
        var reservation = NewReservation();

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void MarkNoShow_MarksAPendingReservation()
    {
        var reservation = NewReservation();

        reservation.MarkNoShow();

        Assert.Equal(ReservationStatus.NoShow, reservation.Status);
    }

    private static Reservation NewReservation()
    {
        return new Reservation(1, "receiver", claimRequestId: null, quantity: 1, pickupCode: "CODE1");
    }
}
