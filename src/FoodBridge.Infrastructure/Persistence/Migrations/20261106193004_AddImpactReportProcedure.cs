using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImpactReportProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR ALTER PROCEDURE sp_GetImpactReport
                    @From datetime2,
                    @To datetime2,
                    @AreaId int = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        s.Id AS ShopId,
                        s.Name AS ShopName,
                        a.Id AS AreaId,
                        a.Name AS AreaName,
                        CAST(SUM(CASE WHEN r.Status = N'Collected' THEN r.Quantity * l.WeightPerUnitKg ELSE 0 END) AS decimal(18,3)) AS KilosSaved,
                        CAST(SUM(CASE WHEN r.Status = N'Collected' THEN r.Quantity ELSE 0 END) AS int) AS CollectedUnits,
                        CAST(SUM(CASE WHEN r.Status = N'NoShow' THEN r.Quantity ELSE 0 END) AS int) AS NoShowUnits,
                        CAST(CASE
                            WHEN SUM(CASE WHEN r.Status IN (N'Collected', N'NoShow') THEN r.Quantity ELSE 0 END) = 0 THEN 0
                            ELSE 1.0 * SUM(CASE WHEN r.Status = N'Collected' THEN r.Quantity ELSE 0 END)
                                 / SUM(CASE WHEN r.Status IN (N'Collected', N'NoShow') THEN r.Quantity ELSE 0 END)
                        END AS decimal(9,4)) AS CollectionRate,
                        CAST(CASE
                            WHEN SUM(CASE WHEN r.Status IN (N'Collected', N'NoShow') THEN r.Quantity ELSE 0 END) = 0 THEN 0
                            ELSE 1.0 * SUM(CASE WHEN r.Status = N'NoShow' THEN r.Quantity ELSE 0 END)
                                 / SUM(CASE WHEN r.Status IN (N'Collected', N'NoShow') THEN r.Quantity ELSE 0 END)
                        END AS decimal(9,4)) AS NoShowRate
                    FROM Reservations r
                    INNER JOIN Listings l ON l.Id = r.ListingId
                    INNER JOIN Shops s ON s.Id = l.ShopId
                    INNER JOIN Areas a ON a.Id = s.AreaId
                    WHERE r.CreatedAt >= @From
                      AND r.CreatedAt < @To
                      AND (@AreaId IS NULL OR s.AreaId = @AreaId)
                    GROUP BY s.Id, s.Name, a.Id, a.Name
                    ORDER BY s.Name;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetImpactReport;");
        }
    }
}
