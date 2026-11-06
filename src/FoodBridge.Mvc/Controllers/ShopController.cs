using FoodBridge.Mvc.Api;
using FoodBridge.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

[Authorize(Roles = "Shop")]
public sealed class ShopController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public ShopController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _api.GetMyListingsAsync(cancellationToken));
        }
        catch (ApiException exception)
        {
            TempData["Error"] = exception.Message;
            return View(new ListingPage([], 1, 50, 0));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The FoodBridge API is not running.";
            return View(new ListingPage([], 1, 50, 0));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(await NewListingFormAsync(cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateListingViewModel model, CancellationToken cancellationToken)
    {
        model.Categories = await LoadCategoriesAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var created = await _api.CreateListingAsync(new NewListing
            {
                Title = model.Title,
                Quantity = model.Quantity,
                WeightPerUnitKg = model.WeightPerUnitKg,
                RequestWindowEndsAt = LebanonTime.ToUtc(model.RequestWindowEndsAt),
                PickupFrom = LebanonTime.ToUtc(model.PickupFrom),
                PickupUntil = LebanonTime.ToUtc(model.PickupUntil),
                CategoryIds = model.CategoryIds
            }, cancellationToken);
            TempData["Message"] = $"{created.Title} is posted.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The FoodBridge API is not running.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Allocate(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _api.AllocateAsync(id, cancellationToken);
            var winners = result.Lines.Where(line => line.GrantedQuantity > 0).ToList();
            TempData["Message"] = winners.Count == 0
                ? $"Allocated. {result.RemainingQuantity} units are left for direct reserve."
                : $"Allocated. {winners.Count} pickup code{(winners.Count == 1 ? "" : "s")} created. {result.RemainingQuantity} units left.";
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _api.CloseListingAsync(id, cancellationToken);
            TempData["Message"] = $"Listing closed. {result.NoShowCount} no-show{(result.NoShowCount == 1 ? "" : "s")}.";
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Collect(CollectForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter the pickup code.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _api.CollectAsync(form.PickupCode, cancellationToken);
            TempData["Message"] = "Pickup confirmed.";
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

    private async Task<CreateListingViewModel> NewListingFormAsync(CancellationToken cancellationToken)
    {
        var now = LebanonTime.Now();
        return new CreateListingViewModel
        {
            Categories = await LoadCategoriesAsync(cancellationToken),
            RequestWindowEndsAt = now.AddHours(6),
            PickupFrom = now.AddHours(7),
            PickupUntil = now.AddHours(9)
        };
    }

    private async Task<IReadOnlyList<LookupItem>> LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return (await _api.GetLookupsAsync(cancellationToken)).Categories;
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (ApiException)
        {
            return [];
        }
    }
}
