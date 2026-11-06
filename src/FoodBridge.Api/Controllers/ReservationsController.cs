using FoodBridge.Application.Reservations;
using FoodBridge.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservations;
    private readonly ICurrentUser _currentUser;

    public ReservationsController(IReservationService reservations, ICurrentUser currentUser)
    {
        _reservations = reservations;
        _currentUser = currentUser;
    }

    [Authorize(Policy = "CanPostFood")]
    [HttpPost("collect")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Collect(
        CollectReservationRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _reservations.CollectAsync(_currentUser.UserId!, request.PickupCode, cancellationToken));
    }

    [Authorize(Policy = "CanClaim")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelMine(int id, CancellationToken cancellationToken)
    {
        await _reservations.CancelAsync(_currentUser.UserId!, id, cancellationToken);
        return NoContent();
    }
}
