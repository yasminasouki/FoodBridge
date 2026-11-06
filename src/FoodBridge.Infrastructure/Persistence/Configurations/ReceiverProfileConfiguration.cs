using FoodBridge.Domain.Entities;
using FoodBridge.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class ReceiverProfileConfiguration : IEntityTypeConfiguration<ReceiverProfile>
{
    public void Configure(EntityTypeBuilder<ReceiverProfile> builder)
    {
        builder.HasKey(profile => profile.UserId);
        builder.Property(profile => profile.UserId).HasMaxLength(450);
        builder.Property(profile => profile.ReceiverType).HasConversion<string>().HasMaxLength(20);
        builder.Property(profile => profile.ReliabilityScore).HasDefaultValue(ReceiverProfile.StartingScore);
        builder.Property(profile => profile.IsApproved).HasDefaultValue(false);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
