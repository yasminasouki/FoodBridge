using FoodBridge.Mvc.Api;
using FoodBridge.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

public sealed class ListingsController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public ListingsController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? areaId, int? categoryId, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = new BrowseViewModel
        {
            AreaId = areaId,
            CategoryId = categoryId,
            Page = page < 1 ? 1 : page
        };

        try
        {
            model.Lookups = await _api.GetLookupsAsync(cancellationToken);
            model.Listings = await _api.SearchListingsAsync(areaId, categoryId, model.Page, cancellationToken);
        }
        catch (ApiException exception)
        {
            model.Error = exception.Message;
        }
        catch (HttpRequestException)
        {
            model.Error = "The FoodBridge API is not running.";
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _api.GetListingAsync(id, cancellationToken);
            return View(listing);
        }
        catch (ApiException exception) when (exception.StatusCode == StatusCodes.Status404NotFound)
        {
            return View("Missing");
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The FoodBridge API is not running.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestFood(int id, QuantityForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a quantity of at least 1.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await SendAsync(() => _api.RequestAsync(id, form.Quantity, cancellationToken), "Your request was sent.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(int id, QuantityForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a quantity of at least 1.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await SendAsync(() => _api.ReserveAsync(id, form.Quantity, cancellationToken), "The leftover food is reserved. Your pickup code is on My Activity.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinWaitlist(int id, QuantityForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a quantity of at least 1.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await SendAsync(
            () => _api.JoinWaitlistAsync(id, form.Quantity, cancellationToken),
            "You are on the waitlist. If food is released, the update appears under Updates.");
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task SendAsync(Func<Task> call, string success)
    {
        try
        {
            await call();
            TempData["Message"] = success;
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The FoodBridge API is not running.";
        }
    }
}
