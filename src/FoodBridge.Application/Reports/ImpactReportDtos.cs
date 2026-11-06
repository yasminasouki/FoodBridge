namespace FoodBridge.Application.Reports;

public sealed class ImpactReportRow
{
    public int ShopId { get; set; }

    public string ShopName { get; set; } = string.Empty;

    public int AreaId { get; set; }

    public string AreaName { get; set; } = string.Empty;

    public decimal KilosSaved { get; set; }

    public int CollectedUnits { get; set; }

    public int NoShowUnits { get; set; }

    public decimal CollectionRate { get; set; }

    public decimal NoShowRate { get; set; }
}

public sealed record ImpactReportDto(
    int ShopId,
    string ShopName,
    int AreaId,
    string AreaName,
    decimal KilosSaved,
    int CollectedUnits,
    int NoShowUnits,
    decimal CollectionRate,
    decimal NoShowRate);
