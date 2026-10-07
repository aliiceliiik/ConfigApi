using System.Security.Claims;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Enums;
using ConfigApi.Mvc.Models;
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly IApiClient _api;
    private readonly ITokenStore _tokens;
    private readonly ITenantSelection _selection;

    public AccountController(IApiClient api, ITokenStore tokens, ITenantSelection selection)
    {
        _api = api;
        _tokens = tokens;
        _selection = selection;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Products");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<TokenResponse>("/api/auth/login",
            new LoginRequest { Email = model.Email, Password = model.Password });

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Giriş başarısız.");
            return View(model);
        }

        var tokens = result.Data;
        _tokens.Save(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt);
        _selection.Clear();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, tokens.FullName),
            new(ClaimTypes.Email, model.Email),
            new(ClaimTypes.Role, tokens.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        if (tokens.Role == UserRole.SuperAdmin)
            return RedirectToAction("Index", "Tenants");

        if (tokens.Role == UserRole.TenantAdmin)
            return RedirectToAction("Orders", "Admin");

        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = _tokens.GetRefreshToken();
        if (!string.IsNullOrEmpty(refreshToken))
            await _api.PostAsync<object>("/api/auth/logout",
                new RefreshRequest { RefreshToken = refreshToken });

        _tokens.Clear();
        _selection.Clear();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
