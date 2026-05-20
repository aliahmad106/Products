using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string AccessTokenCookie = "access_token";
    private const string RefreshTokenCookie = "refresh_token";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var response = _authService.Authenticate(request);
        SetAuthCookies(response);

        return Ok(new { expiresAt = response.ExpiresAt });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { message = "No refresh token provided." });
        }

        var response = _authService.RefreshToken(refreshToken);
        SetAuthCookies(response);

        return Ok(new { expiresAt = response.ExpiresAt });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            _authService.RevokeRefreshToken(refreshToken);
        }

        Response.Cookies.Delete(AccessTokenCookie);
        Response.Cookies.Delete(RefreshTokenCookie);

        return NoContent();
    }

    private void SetAuthCookies(LoginResponse response)
    {
        var isProduction = !HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>().IsDevelopment();

        Response.Cookies.Append(AccessTokenCookie, response.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Expires = response.ExpiresAt,
            Path = "/"
        });

        Response.Cookies.Append(RefreshTokenCookie, response.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Expires = response.RefreshTokenExpiresAt,
            Path = "/api/auth"
        });
    }
}
