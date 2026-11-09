using FoodBridge.Application.Notifications;
using FoodBridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Notifications;

public sealed class AccountNameQuery : IAccountNameQuery
{
    private readonly AppDbContext _context;

    public AccountNameQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetFullNameAsync(string userId, CancellationToken cancellationToken = default)
    {
        var name = await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.FullName)
            .FirstOrDefaultAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(name) ? "Someone" : name;
    }
}
