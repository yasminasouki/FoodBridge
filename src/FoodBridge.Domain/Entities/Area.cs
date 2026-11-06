using FoodBridge.Domain.Common;

namespace FoodBridge.Domain.Entities;

public sealed class Area : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;

    private Area()
    {
    }

    public Area(string name, string district)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Area name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(district))
        {
            throw new ArgumentException("District is required.", nameof(district));
        }

        Name = name.Trim();
        District = district.Trim();
    }
}
