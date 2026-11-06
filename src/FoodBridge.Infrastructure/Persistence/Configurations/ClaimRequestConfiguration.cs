using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class ClaimRequestConfiguration : IEntityTypeConfiguration<ClaimRequest>
{
    public void Configure(EntityTypeBuilder<ClaimRequest> builder)
    {
        builder.Property(request => request.ReceiverId).HasMaxLength(450).IsRequired();
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(request => new { request.ListingId, request.ReceiverId })
            .IsUnique();

        builder.HasOne<Listing>()
            .WithMany()
            .HasForeignKey(request => request.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReceiverProfile>()
            .WithMany()
            .HasForeignKey(request => request.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
