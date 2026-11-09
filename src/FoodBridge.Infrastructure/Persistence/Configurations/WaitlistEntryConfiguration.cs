using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.Property(entry => entry.ReceiverId).HasMaxLength(450).IsRequired();
        builder.Property(entry => entry.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(entry => new { entry.ListingId, entry.ReceiverId }).IsUnique();

        builder.HasOne<Listing>()
            .WithMany()
            .HasForeignKey(entry => entry.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReceiverProfile>()
            .WithMany()
            .HasForeignKey(entry => entry.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
