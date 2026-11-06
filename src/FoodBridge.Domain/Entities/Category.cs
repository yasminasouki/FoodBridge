using FoodBridge.Domain.Common;

namespace FoodBridge.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    private Category()
    {
    }

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        Name = name.Trim();
    }
}
