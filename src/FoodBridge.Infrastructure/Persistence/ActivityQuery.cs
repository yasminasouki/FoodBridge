using FoodBridge.Application.Activity;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence;

public sealed class ActivityQuery : IActivityQuery
{
    private readonly AppDbContext _context;

    public ActivityQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MyRequestDto>> ListRequestsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimRequests
            .AsNoTracking()
            .Where(request => request.ReceiverId == userId)
            .OrderByDescending(request => request.Id)
            .Select(request => new MyRequestDto(
                request.Id,
                request.ListingId,
                _context.Listings.Where(listing => listing.Id == request.ListingId).Select(listing => listing.Title).First(),
                request.RequestedQuantity,
                request.AllocatedQuantity,
                request.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MyReservationDto>> ListReservationsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.ReceiverId == userId)
            .OrderByDescending(reservation => reservation.Id)
            .Select(reservation => new MyReservationDto(
                reservation.Id,
                reservation.ListingId,
                _context.Listings.Where(listing => listing.Id == reservation.ListingId).Select(listing => listing.Title).First(),
                reservation.Quantity,
                reservation.PickupCode,
                reservation.Status.ToString(),
                reservation.CollectedAt))
            .ToListAsync(cancellationToken);
    }
}
