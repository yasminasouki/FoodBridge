using FoodBridge.Application.Auth;
using FoodBridge.Application.Listings;
using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;
using FoodBridge.Infrastructure.Identity;
using FoodBridge.Infrastructure.Persistence;
using FoodBridge.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodBridge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FoodBridge")
            ?? throw new InvalidOperationException("Connection string 'FoodBridge' is missing.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IShopRepository, ShopRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IClaimRequestRepository, ClaimRequestRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
