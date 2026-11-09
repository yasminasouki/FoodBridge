using FoodBridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBridge.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.Property(notification => notification.UserId).HasMaxLength(450).IsRequired();
        builder.Property(notification => notification.Message).HasMaxLength(400).IsRequired();
        builder.HasIndex(notification => new { notification.UserId, notification.CreatedAt });
    }
}
