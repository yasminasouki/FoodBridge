using FoodBridge.Domain.Entities;
using FoodBridge.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(user => user.FullName).HasMaxLength(100).IsRequired();

        builder.HasOne<Area>()
            .WithMany()
            .HasForeignKey(user => user.AreaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
