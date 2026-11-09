using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize]
public sealed class ProfileController(RaceDayApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await api.GetAsync<UserDto>("api/users/profile", Token());
        if (!result.Succeeded || result.Value is null)
        {
            ViewData["ApiError"] = result.Error;
            return View(new ProfilePage());
        }
        return View(new ProfilePage
        {
            Email = result.Value.Email,
            Role = result.Value.Role,
            FirstName = result.Value.FirstName,
            LastName = result.Value.LastName,
            ProfilePictureUrl = result.Value.ProfilePictureUrl
        });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfilePage page)
    {
        if (!ModelState.IsValid)
            return View(page);

        var result = await api.PutAsync<UserDto>("api/users/profile",
            new ProfileUpdateRequest(page.FirstName, page.LastName), Token());
        if (!result.Succeeded || result.Value is null)
        {
            ViewData["ApiError"] = result.Error;
            return View(page);
        }

        page.Email = result.Value.Email;
        page.Role = result.Value.Role;
        page.ProfilePictureUrl = result.Value.ProfilePictureUrl;
        if (page.ProfilePicture is { Length: > 0 })
        {
            var upload = await api.PostFileAsync<UserDto>("api/users/profile/picture", page.ProfilePicture, Token());
            if (!upload.Succeeded || upload.Value is null)
            {
                ViewData["ApiError"] = $"Profile details were saved, but the picture upload failed: {upload.Error}";
                return View(page);
            }
            page.ProfilePictureUrl = upload.Value.ProfilePictureUrl;
        }

        TempData["Notice"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }


    private string? Token() => HttpContext.Session.GetString("RaceDayApiToken");
}
