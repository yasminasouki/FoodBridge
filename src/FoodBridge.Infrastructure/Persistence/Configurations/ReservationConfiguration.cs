using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.Property(reservation => reservation.ReceiverId).HasMaxLength(450).IsRequired();
        builder.Property(reservation => reservation.PickupCode).HasMaxLength(32).IsRequired();
        builder.Property(reservation => reservation.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(reservation => reservation.PickupCode).IsUnique();
        builder.HasIndex(reservation => reservation.ClaimRequestId).IsUnique();

        builder.HasOne<Listing>()
            .WithMany()
            .HasForeignKey(reservation => reservation.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReceiverProfile>()
            .WithMany()
            .HasForeignKey(reservation => reservation.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClaimRequest>()
            .WithMany()
            .HasForeignKey(reservation => reservation.ClaimRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
