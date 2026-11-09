using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public sealed class AccountController(RaceDayApiClient api) : Controller
{
    [AllowAnonymous]
    public IActionResult Login() => View(new LoginPage());


    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginPage page, string? returnUrl)
    {
        if (!ModelState.IsValid)
            return View(page);

        var result = await api.PostAsync<AuthDto>("api/auth/login", new LoginRequest(page.Email, page.Password));
        if (!result.Succeeded || result.Value is null)
        {
            ViewData["ApiError"] = result.Error ?? "Sign in failed. Check your details and try again.";
            return View(page);
        }

        var user = result.Value.User;
        HttpContext.Session.SetString("RaceDayApiToken", result.Value.Token);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = result.Value.ExpiresAtUtc });

        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl!);

        return user.Role == "Organiser"
            ? RedirectToAction("Index", "Organiser")
            : RedirectToAction("Index", "Events");
    }


    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterPage());


    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterPage page)
    {
        if (!ModelState.IsValid)
            return View(page);

        var result = await api.PostAsync<UserDto>("api/auth/register",
            new RegisterRequest(page.FirstName, page.LastName, page.Email, page.Password, page.Role));
        if (!result.Succeeded)
        {
            ViewData["ApiError"] = result.Error ?? "Registration failed. Check your details and try again.";
            return View(page);
        }

        TempData["Notice"] = "Account created. Sign in to continue.";
        return RedirectToAction(nameof(Login));
    }


    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }


    [AllowAnonymous]
    public IActionResult Denied() => View();
}
