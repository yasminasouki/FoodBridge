using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(AppDbContext context)
        : base(context)
    {
    }
}
