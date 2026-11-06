using FoodBridge.Mvc.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

[Authorize(Roles = "Admin")]
public sealed class ApprovalsController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public ApprovalsController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _api.GetApprovalsAsync(cancellationToken));
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
            return View(Array.Empty<PendingApproval>());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The FoodBridge API is not running.";
            return View(Array.Empty<PendingApproval>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string userId, CancellationToken cancellationToken)
    {
        try
        {
            await _api.ApproveAsync(userId, cancellationToken);
            TempData["Message"] = "Approved.";
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The FoodBridge API is not running.";
        }

        return RedirectToAction(nameof(Index));
    }
}
