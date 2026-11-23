using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersatileApi.Data;
using VersatileApi.Models;
using VersatileApi.Services;

namespace VersatileApi.Controllers;

public record LoginRequest(string Username, string Password);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly PasswordHasher<AdminUser> _hasher = new();

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid username or password." });

        var (token, expires) = _tokens.Create(user.Username);
        return Ok(new { token, expires, username = user.Username });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Username == User.Identity!.Name);
        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, req.CurrentPassword) == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Current password is incorrect." });

        user.PasswordHash = _hasher.HashPassword(user, req.NewPassword);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Password changed." });
    }
}
