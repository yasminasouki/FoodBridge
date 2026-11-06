using FoodBridge.Application.Security;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FoodBridge.Infrastructure.Identity;

public static class DevelopmentSeeder
{
    public const string DemoPassword = "Password1!";

    public static async Task SeedAsync(IServiceProvider services, bool isDevelopment)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        if (!isDevelopment)
        {
            return;
        }

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));
            }
        }

        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        await EnsureUserAsync(users, context, "admin@foodbridge.test", "Admin User", Roles.Admin, areaId: 1, shop: null, profile: null);
        await EnsureUserAsync(
            users,
            context,
            "shop@foodbridge.test",
            "Sunrise Bakery",
            Roles.Shop,
            areaId: 1,
            shop: ("Sunrise Bakery", "Hamra Street"),
            profile: null);
        await EnsureUserAsync(
            users,
            context,
            "receiver@foodbridge.test",
            "Layla Receiver",
            Roles.Receiver,
            areaId: 1,
            shop: null,
            profile: (ReceiverType.Individual, 1, approved: true));
        await EnsureUserAsync(
            users,
            context,
            "ngo@foodbridge.test",
            "Community Kitchen",
            Roles.Ngo,
            areaId: 2,
            shop: null,
            profile: (ReceiverType.Ngo, 80, approved: true));
        await EnsureUserAsync(
            users,
            context,
            "ngo.pending@foodbridge.test",
            "Pending Shelter",
            Roles.Ngo,
            areaId: 3,
            shop: null,
            profile: (ReceiverType.Ngo, 12, approved: false));
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> users,
        AppDbContext context,
        string email,
        string fullName,
        string role,
        int areaId,
        (string Name, string Address)? shop,
        (ReceiverType Type, int PeopleServed, bool approved)? profile)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                AreaId = areaId,
                EmailConfirmed = true
            };
            var created = await users.CreateAsync(user, DemoPassword);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", created.Errors.Select(error => error.Description)));
            }

            await users.AddToRoleAsync(user, role);
        }

        if (shop is not null && !await context.Shops.AnyAsync(item => item.OwnerUserId == user.Id))
        {
            context.Shops.Add(new Shop(user.Id, areaId, shop.Value.Name, shop.Value.Address));
        }

        if (profile is not null && !await context.ReceiverProfiles.AnyAsync(item => item.UserId == user.Id))
        {
            var receiver = new ReceiverProfile(user.Id, profile.Value.Type, profile.Value.PeopleServed);
            if (profile.Value.approved)
            {
                receiver.Approve();
            }

            context.ReceiverProfiles.Add(receiver);
        }

        await context.SaveChangesAsync();
    }
}
