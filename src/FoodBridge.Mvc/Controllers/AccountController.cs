using System.Security.Claims;
using FoodBridge.Mvc.Api;
using FoodBridge.Mvc.Models;
using FoodBridge.Mvc.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Mvc.Controllers;

public sealed class AccountController : Controller
{
    private readonly IFoodBridgeApiClient _api;

    public AccountController(IFoodBridgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Denied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _api.LoginAsync(model.Email, model.Password, cancellationToken);
            await SignInAsync(result.Token);
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

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Listings");
    }

    [HttpGet]
    public async Task<IActionResult> Register(CancellationToken cancellationToken)
    {
        return View(await NewRegisterFormAsync(cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        model.Areas = await LoadAreasAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _api.RegisterAsync(new RegisterAccount
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role,
                AreaId = model.AreaId,
                ShopName = model.ShopName,
                Address = model.Address,
                PeopleServed = model.PeopleServed
            }, cancellationToken);
            var login = await _api.LoginAsync(model.Email, model.Password, cancellationToken);
            await SignInAsync(login.Token);
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

        return RedirectToAction("Index", "Listings");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Listings");
    }

    private async Task<RegisterViewModel> NewRegisterFormAsync(CancellationToken cancellationToken)
    {
        return new RegisterViewModel { Areas = await LoadAreasAsync(cancellationToken) };
    }

    private async Task<IReadOnlyList<LookupItem>> LoadAreasAsync(CancellationToken cancellationToken)
    {
        try
        {
            return (await _api.GetLookupsAsync(cancellationToken)).Areas;
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

    private async Task SignInAsync(string token)
    {
        var identity = AccessToken.ReadIdentity(token);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
