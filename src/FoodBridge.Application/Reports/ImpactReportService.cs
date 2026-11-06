using FoodBridge.Application.Persistence;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Application.Reports;

public sealed class ImpactReportService : IImpactReportService
{
    private readonly IImpactReportQuery _reports;
    private readonly IShopRepository _shops;

    public ImpactReportService(IImpactReportQuery reports, IShopRepository shops)
    {
        _reports = reports;
        _shops = shops;
    }

    public async Task<IReadOnlyList<ImpactReportDto>> GetAsync(
        string userId,
        bool isAdmin,
        DateTime from,
        DateTime to,
        int? areaId,
        CancellationToken cancellationToken = default)
    {
        if (from >= to)
        {
            throw new DomainException("The report start must be before the end.");
        }

        Shop? shop = null;
        if (!isAdmin)
        {
            shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
                ?? throw new ForbiddenException("Only a shop or an admin can see the impact report.");
        }

        var rows = await _reports.QueryAsync(from, to, areaId, cancellationToken);
        if (shop is not null)
        {
            rows = rows.Where(row => row.ShopId == shop.Id).ToList();
        }

        return rows.Select(row => new ImpactReportDto(
            row.ShopId,
            row.ShopName,
            row.AreaId,
            row.AreaName,
            row.KilosSaved,
            row.CollectedUnits,
            row.NoShowUnits,
            row.CollectionRate,
            row.NoShowRate)).ToList();
    }
}
