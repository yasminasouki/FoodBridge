namespace FoodBridge.Application.Reports;

public interface IImpactReportQuery
{
    Task<IReadOnlyList<ImpactReportRow>> QueryAsync(
        DateTime from,
        DateTime to,
        int? areaId,
        CancellationToken cancellationToken = default);
}

public interface IImpactReportService
{
    Task<IReadOnlyList<ImpactReportDto>> GetAsync(
        string userId,
        bool isAdmin,
        DateTime from,
        DateTime to,
        int? areaId,
        CancellationToken cancellationToken = default);
}
