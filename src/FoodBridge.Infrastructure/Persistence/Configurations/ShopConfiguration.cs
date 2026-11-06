using FoodBridge.Domain.Entities;
using FoodBridge.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.Property(shop => shop.OwnerUserId).HasMaxLength(450).IsRequired();
        builder.Property(shop => shop.Name).HasMaxLength(150).IsRequired();
        builder.Property(shop => shop.Address).HasMaxLength(250).IsRequired();
        builder.Property(shop => shop.IsApproved).HasDefaultValue(false);

        builder.HasOne<Area>()
            .WithMany()
            .HasForeignKey(shop => shop.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(shop => shop.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
