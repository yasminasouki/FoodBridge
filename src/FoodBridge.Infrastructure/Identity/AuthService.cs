using FoodBridge.Application.Auth;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;
using FoodBridge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _users;
    private readonly AppDbContext _context;
    private readonly ITokenService _tokens;

    public AuthService(UserManager<AppUser> users, AppDbContext context, ITokenService tokens)
    {
        _users = users;
        _context = context;
        _tokens = tokens;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = Roles.All.FirstOrDefault(name =>
            string.Equals(name, request.Role, StringComparison.OrdinalIgnoreCase)
            && name != Roles.Admin)
            ?? throw new ConflictException("Role must be Shop, Receiver, or Ngo.");

        var areaExists = await _context.Areas.AnyAsync(area => area.Id == request.AreaId, cancellationToken);
        if (!areaExists)
        {
            throw new NotFoundException("Area was not found.");
        }

        var user = new AppUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            AreaId = request.AreaId
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var created = await _users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            var duplicate = created.Errors.Any(error =>
                error.Code is "DuplicateEmail" or "DuplicateUserName");
            var message = string.Join(" ", created.Errors.Select(error => error.Description));
            throw duplicate
                ? new ConflictException("An account with this email already exists.")
                : new DomainException(message);
        }

        var roleResult = await _users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            throw new DomainException(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }

        if (role == Roles.Shop)
        {
            _context.Shops.Add(new Shop(user.Id, request.AreaId, request.ShopName!.Trim(), request.Address!.Trim()));
        }
        else if (role == Roles.Ngo)
        {
            _context.ReceiverProfiles.Add(new ReceiverProfile(user.Id, ReceiverType.Ngo, request.PeopleServed!.Value));
        }
        else
        {
            _context.ReceiverProfiles.Add(new ReceiverProfile(user.Id, ReceiverType.Individual, peopleServed: 1));
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new RegisterResponse(user.Id, user.Email, role);
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await _users.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Email or password is wrong.");
        }

        var roles = await _users.GetRolesAsync(user);
        var (token, expires) = _tokens.CreateToken(user.Id, user.Email ?? request.Email, user.FullName, roles.ToList());
        return new LoginResponse(token, expires);
    }
}
