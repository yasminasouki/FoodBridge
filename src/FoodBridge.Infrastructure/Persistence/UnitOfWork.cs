using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IListingRepository listings,
        IClaimRequestRepository claimRequests,
        IReservationRepository reservations)
    {
        _context = context;
        Listings = listings;
        ClaimRequests = claimRequests;
        Reservations = reservations;
    }

    public IListingRepository Listings { get; }

    public IClaimRequestRepository ClaimRequests { get; }

    public IReservationRepository Reservations { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("This listing changed. Load it again and try once more.");
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new ConflictException("This conflicts with an existing row.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sql && sql.Number is 2601 or 2627;
    }
}
