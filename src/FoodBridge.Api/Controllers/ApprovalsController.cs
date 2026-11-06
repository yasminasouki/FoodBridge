using FoodBridge.Application.Auth;
using FoodBridge.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/approvals")]
public sealed class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvals;

    public ApprovalsController(IApprovalService approvals)
    {
        _approvals = approvals;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PendingApprovalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PendingApprovalResponse>>> GetPending(
        CancellationToken cancellationToken)
    {
        return Ok(await _approvals.GetPendingAsync(cancellationToken));
    }

    [HttpPatch("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(string userId, CancellationToken cancellationToken)
    {
        await _approvals.ApproveAsync(userId, cancellationToken);
        return NoContent();
    }
}
