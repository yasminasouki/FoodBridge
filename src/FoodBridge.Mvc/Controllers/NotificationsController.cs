using FoodBridge.Mvc;
using FoodBridge.Mvc.Api;
using FoodBridge.Mvc.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

[Authorize]
public sealed class NotificationsController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public NotificationsController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        try
        {
            var notices = await _api.GetNotificationsAsync(cancellationToken);
            return Json(notices.Select(notice => new
            {
                notice.Id,
                notice.Message,
                createdAt = LebanonTime.Format(notice.CreatedAt),
                notice.IsRead,
                href = Url.Action(nameof(Open), new { id = notice.Id })
            }));
        }
        catch (ApiException)
        {
            return Json(Array.Empty<object>());
        }
        catch (HttpRequestException)
        {
            return Json(Array.Empty<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Open(int id, CancellationToken cancellationToken)
    {
        try
        {
            var notice = await _api.MarkNotificationReadAsync(id, cancellationToken);
            if (notice.ListingId is int listingId)
            {
                return RedirectToAction("Details", "Listings", new { id = listingId });
            }

            return RedirectToAction("Index", "Listings");
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction("Index", "Listings");
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The FoodBridge API is not running.";
            return RedirectToAction("Index", "Listings");
        }
    }

    [HttpGet]
    public IActionResult Token()
    {
        var token = User.FindFirst(AccessToken.ClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        return Content(token, "text/plain");
    }
}
