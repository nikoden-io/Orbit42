using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orback.Application.Dtos;
using Orback.Application.Interfaces;
using Orback.Domain.Entities;
using Orback.Infrastructure.Persistence;

namespace Orback.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        AppDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        var userExists = await _userManager.FindByEmailAsync(dto.Email);
        if (userExists != null)
            return BadRequest("User already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Unauthorized("Invalid credentials");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshTokenString();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        SetRefreshTokenCookie(refreshTokenString, refreshToken.ExpiresAt);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken =
                refreshTokenString, // included for now, even if not directly used (using http-only cookie for now)
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshTokenCookie = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshTokenCookie))
            return Unauthorized("No refresh token provided");

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenCookie);

        if (refreshToken == null || !refreshToken.IsActive)
            return Unauthorized("Invalid or expired refresh token");

        // Token rotation: revoke old token and create a new one
        refreshToken.RevokedAt = DateTime.UtcNow;

        var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
        if (user == null)
            return Unauthorized("User not found");

        var newRefreshTokenString = _tokenService.GenerateRefreshTokenString();
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        SetRefreshTokenCookie(newRefreshTokenString, newRefreshToken.ExpiresAt);

        var newAccessToken = _tokenService.GenerateAccessToken(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Invalidate current refresh token by reading the cookie and revoking it
        var refreshTokenCookie = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshTokenCookie))
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenCookie && rt.IsActive);
            if (refreshToken != null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // Remove the cookie
        Response.Cookies.Delete("refresh_token");

        return Ok(new { message = "Logged out successfully" });
    }

    private void SetRefreshTokenCookie(string token, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, //TODO: Set true in production or if HTTPS is active
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        };
        Response.Cookies.Append("refresh_token", token, cookieOptions);
    }
}