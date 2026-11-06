using FoodBridge.Domain.Common;

namespace FoodBridge.Domain.Entities;

public sealed class Shop : BaseEntity
{
    public string OwnerUserId { get; private set; } = string.Empty;
    public int AreaId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; }

    private Shop()
    {
    }

    public Shop(string ownerUserId, int areaId, string name, string address)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId))
        {
            throw new ArgumentException("Owner is required.", nameof(ownerUserId));
        }

        if (areaId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(areaId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Shop name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address is required.", nameof(address));
        }

        OwnerUserId = ownerUserId;
        AreaId = areaId;
        Name = name.Trim();
        Address = address.Trim();
    }

    public void Approve()
    {
        IsApproved = true;
    }
}
