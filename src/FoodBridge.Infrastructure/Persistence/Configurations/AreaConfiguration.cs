using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.Property(area => area.Name).HasMaxLength(100).IsRequired();
        builder.Property(area => area.District).HasMaxLength(100).IsRequired();

        builder.HasData(
            new { Id = 1, Name = "Hamra", District = "Beirut" },
            new { Id = 2, Name = "Achrafieh", District = "Beirut" },
            new { Id = 3, Name = "Tripoli", District = "North" },
            new { Id = 4, Name = "Saida", District = "South" },
            new { Id = 5, Name = "Zahle", District = "Bekaa" });
    }
}
