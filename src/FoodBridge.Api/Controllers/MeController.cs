using FoodBridge.Application.Activity;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Api.Controllers;

[ApiController]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    private readonly IActivityService _activity;
    private readonly ICurrentUser _currentUser;

    public MeController(IActivityService activity, ICurrentUser currentUser)
    {
        _activity = activity;
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet("activity")]
    [ProducesResponseType(typeof(ActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ActivityDto>> Activity(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedException("Log in to see your activity.");
        }

        return Ok(await _activity.GetAsync(_currentUser.UserId, cancellationToken));
    }
}
