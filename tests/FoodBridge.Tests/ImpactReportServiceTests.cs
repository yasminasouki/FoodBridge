using FoodBridge.Application.Persistence;
using FoodBridge.Application.Reports;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Tests;

public class ImpactReportServiceTests
{
    [Fact]
    public async Task Get_RejectsARangeThatEndsBeforeItStarts()
    {
        var service = new ImpactReportService(new FakeReports([]), new FakeShops(null));

        await Assert.ThrowsAsync<DomainException>(() => service.GetAsync(
            "shop",
            isAdmin: false,
            from: new DateTime(2026, 11, 7),
            to: new DateTime(2026, 11, 6),
            areaId: null));
    }

    [Fact]
    public async Task Get_KeepsOnlyTheCallingShop()
    {
        var rows = new[]
        {
            Row(shopId: 1, shopName: "Sunrise Bakery"),
            Row(shopId: 2, shopName: "Other Bakery")
        };
        var shop = new Shop("owner", 1, "Sunrise Bakery", "Hamra Street");
        typeof(Shop).BaseType!.GetProperty("Id")!.SetValue(shop, 1);
        var service = new ImpactReportService(new FakeReports(rows), new FakeShops(shop));

        var result = await service.GetAsync(
            "owner",
            isAdmin: false,
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 8),
            areaId: null);

        var row = Assert.Single(result);
        Assert.Equal("Sunrise Bakery", row.ShopName);
    }

    [Fact]
    public async Task Get_ReturnsEveryShopForAnAdmin()
    {
        var rows = new[]
        {
            Row(shopId: 1, shopName: "Sunrise Bakery"),
            Row(shopId: 2, shopName: "Other Bakery")
        };
        var service = new ImpactReportService(new FakeReports(rows), new FakeShops(null));

        var result = await service.GetAsync(
            "admin",
            isAdmin: true,
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 8),
            areaId: null);

        Assert.Equal(2, result.Count);
    }

    private static ImpactReportRow Row(int shopId, string shopName) => new()
    {
        ShopId = shopId,
        ShopName = shopName,
        AreaId = 1,
        AreaName = "Hamra",
        KilosSaved = 0.4m,
        CollectedUnits = 1,
        NoShowUnits = 1,
        CollectionRate = 0.5m,
        NoShowRate = 0.5m
    };

    private sealed class FakeReports : IImpactReportQuery
    {
        private readonly IReadOnlyList<ImpactReportRow> _rows;

        public FakeReports(IReadOnlyList<ImpactReportRow> rows) => _rows = rows;

        public Task<IReadOnlyList<ImpactReportRow>> QueryAsync(
            DateTime from,
            DateTime to,
            int? areaId,
            CancellationToken cancellationToken = default) => Task.FromResult(_rows);
    }

    private sealed class FakeShops : IShopRepository
    {
        private readonly Shop? _shop;

        public FakeShops(Shop? shop) => _shop = shop;

        public Task<Shop?> GetByOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_shop);
    }
}
