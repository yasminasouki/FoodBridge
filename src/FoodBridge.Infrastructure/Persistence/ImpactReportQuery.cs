using FoodBridge.Application.Reports;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence;

public sealed class ImpactReportQuery : IImpactReportQuery
{
    private readonly AppDbContext _context;

    public ImpactReportQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ImpactReportRow>> QueryAsync(
        DateTime from,
        DateTime to,
        int? areaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Database
            .SqlQuery<ImpactReportRow>($"EXEC sp_GetImpactReport {from}, {to}, {areaId}")
            .ToListAsync(cancellationToken);
    }
}
