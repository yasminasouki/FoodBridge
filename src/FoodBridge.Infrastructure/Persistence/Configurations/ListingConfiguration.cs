using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.Property(listing => listing.Title).HasMaxLength(150).IsRequired();
        builder.Property(listing => listing.WeightPerUnitKg).HasPrecision(8, 3);
        builder.Property(listing => listing.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(listing => listing.RowVersion).IsRowVersion();

        builder.HasOne<Shop>()
            .WithMany()
            .HasForeignKey(listing => listing.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Listings_RemainingQuantity",
            "[RemainingQuantity] >= 0 AND [RemainingQuantity] <= [OriginalQuantity]"));
    }
}
