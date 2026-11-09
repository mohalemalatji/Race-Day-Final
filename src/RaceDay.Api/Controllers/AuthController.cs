using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(RaceDayDbContext db, IPasswordHasher<User> hasher, IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict(new { message = "An account with this email already exists." });
        var user = new User { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email, Role = request.Role };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "An account with this email already exists." }); }
        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant());
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var expires = DateTime.UtcNow.AddHours(2);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID), new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims,
            expires: expires, signingCredentials: credentials);
        return Ok(new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, ToResponse(user)));
    }


    private UserResponse ToResponse(User user)
    {
        var pictureUrl = string.IsNullOrWhiteSpace(user.ProfilePictureBlobName)
            ? null
            : $"{Request.Scheme}://{Request.Host}/api/images/profile-pictures/{Uri.EscapeDataString(user.ProfilePictureBlobName)}";
        return new UserResponse(user.UserID, user.FirstName, user.LastName, user.Email, user.Role, pictureUrl);
    }
}
