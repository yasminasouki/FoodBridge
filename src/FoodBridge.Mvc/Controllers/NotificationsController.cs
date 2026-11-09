using FoodBridge.Mvc.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

[Authorize(Roles = "Receiver,Ngo")]
public sealed class NotificationsController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public NotificationsController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _api.GetNotificationsAsync(cancellationToken));
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
            return View(Array.Empty<Notice>());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The FoodBridge API is not running.";
            return View(Array.Empty<Notice>());
        }
    }
}
