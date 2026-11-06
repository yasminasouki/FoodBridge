using FoodBridge.Application.Reports;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IImpactReportService _reports;
    private readonly ICurrentUser _currentUser;

    public ReportsController(IImpactReportService reports, ICurrentUser currentUser)
    {
        _reports = reports;
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet("impact")]
    [ProducesResponseType(typeof(IReadOnlyList<ImpactReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ImpactReportDto>>> Impact(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? areaId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedException("Log in to see the impact report.");
        }

        var rows = await _reports.GetAsync(
            _currentUser.UserId,
            User.IsInRole(Roles.Admin),
            from,
            to,
            areaId,
            cancellationToken);
        return Ok(rows);
    }
}
