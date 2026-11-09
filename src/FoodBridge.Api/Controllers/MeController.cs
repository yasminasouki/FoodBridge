using FoodBridge.Application.Activity;
using FoodBridge.Application.Notifications;
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
    private readonly INotificationService _notifications;
    private readonly ICurrentUser _currentUser;

    public MeController(IActivityService activity, INotificationService notifications, ICurrentUser currentUser)
    {
        _activity = activity;
        _notifications = notifications;
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

    [Authorize]
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Notifications(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedException("Log in to see your updates.");
        }

        return Ok(await _notifications.ListAsync(_currentUser.UserId, cancellationToken));
    }

    [Authorize]
    [HttpPost("notifications/{id:int}/read")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> MarkRead(int id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedException("Log in to see your updates.");
        }

        return Ok(await _notifications.MarkReadAsync(_currentUser.UserId, id, cancellationToken));
    }
}
