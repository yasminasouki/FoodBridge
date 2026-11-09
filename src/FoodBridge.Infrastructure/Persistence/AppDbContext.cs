using FoodBridge.Domain.Entities;
using FoodBridge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Area> Areas => Set<Area>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Shop> Shops => Set<Shop>();

    public DbSet<Listing> Listings => Set<Listing>();

    public DbSet<ListingCategory> ListingCategories => Set<ListingCategory>();

    public DbSet<ClaimRequest> ClaimRequests => Set<ClaimRequest>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<ReceiverProfile> ReceiverProfiles => Set<ReceiverProfile>();

    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        var utc = new UtcDateTimeConverter();
        var utcNullable = new NullableUtcDateTimeConverter();

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.ClrType == typeof(DateTime))
            {
                property.SetValueConverter(utc);
            }
            else if (property.ClrType == typeof(DateTime?))
            {
                property.SetValueConverter(utcNullable);
            }
        }
    }
}
