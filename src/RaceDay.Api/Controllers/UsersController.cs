using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Authorize, Route("api/users/profile")]
public sealed class UsersController(RaceDayDbContext db, SqlImageStorage storage) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        var user = await db.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        return user is null ? NotFound() : Ok(ToResponse(user));
    }


    [HttpPut]
    public async Task<ActionResult<UserResponse>> UpdateProfile(ProfileUpdateRequest request)
    {
        var user = await db.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (user is null) return NotFound();
        user.FirstName = request.FirstName.Trim(); user.LastName = request.LastName.Trim();
        await db.SaveChangesAsync();
        return Ok(ToResponse(user));
    }


    [HttpPost("picture"), Consumes("multipart/form-data"), RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<ActionResult<UserResponse>> UploadProfilePicture(IFormFile? file, CancellationToken cancellationToken)
    {
        if (!await SqlImageStorage.IsSupportedImageAsync(file, cancellationToken))
            return BadRequest(new { message = "Choose a JPEG, PNG, or WebP image up to 5 MB." });

        var user = await db.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (user is null) return NotFound();

        var oldBlobName = user.ProfilePictureBlobName;
        user.ProfilePictureBlobName = await storage.UploadAsync(file!, "profile-pictures", cancellationToken);
        await db.SaveChangesAsync();
        await storage.DeleteIfExistsAsync("profile-pictures", oldBlobName, cancellationToken);
        return Ok(ToResponse(user));
    }


    private UserResponse ToResponse(User user)
    {
        var pictureUrl = string.IsNullOrWhiteSpace(user.ProfilePictureBlobName)
            ? null
            : $"{Request.Scheme}://{Request.Host}/api/images/profile-pictures/{Uri.EscapeDataString(user.ProfilePictureBlobName)}";
        return new UserResponse(user.UserID, user.FirstName, user.LastName, user.Email, user.Role, pictureUrl);
    }
}
