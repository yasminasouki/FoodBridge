using FoodBridge.Mvc.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

[Authorize]
public sealed class ActivityController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public ActivityController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _api.GetActivityAsync(cancellationToken));
        }
        catch (ApiException exception)
        {
            ViewData["Error"] = exception.Message;
            return View(new Activity(null, null, null, [], []));
        }
        catch (HttpRequestException)
        {
            ViewData["Error"] = "The FoodBridge API is not running.";
            return View(new Activity(null, null, null, [], []));
        }
    }
}
