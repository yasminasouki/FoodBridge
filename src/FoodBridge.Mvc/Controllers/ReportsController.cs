using FoodBridge.Mvc.Api;
using FoodBridge.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

[Authorize(Roles = "Shop,Admin")]
public sealed class ReportsController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public ReportsController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Impact(DateTime? from, DateTime? to, int? areaId, CancellationToken cancellationToken)
    {
        var today = LebanonTime.Now().Date;
        var model = new ImpactQuery
        {
            From = from ?? today.AddDays(-2),
            To = to ?? today,
            AreaId = areaId
        };

        try
        {
            model.Areas = (await _api.GetLookupsAsync(cancellationToken)).Areas;
            if (model.From.Date <= model.To.Date)
            {
                model.Rows = await _api.GetImpactAsync(
                    LebanonTime.ToUtc(model.From.Date),
                    LebanonTime.ToUtc(model.To.Date.AddDays(1)),
                    User.IsInRole("Admin") ? areaId : null,
                    cancellationToken);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "The start date must be on or before the end date.");
            }
        }
        catch (ApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The FoodBridge API is not running.");
        }

        return View(model);
    }
}
