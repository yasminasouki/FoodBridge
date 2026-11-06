using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(category => category.Name).HasMaxLength(100).IsRequired();

        builder.HasData(
            new { Id = 1, Name = "Bakery" },
            new { Id = 2, Name = "Cooked meals" },
            new { Id = 3, Name = "Fruits and vegetables" },
            new { Id = 4, Name = "Dairy" },
            new { Id = 5, Name = "Vegetarian" });
    }
}
