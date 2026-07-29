using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Auth;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Authenticates a user and issues a short-lived JWT access token plus a longer-lived opaque refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(result);
    }

    /// <summary>Rotates a refresh token: the presented token is revoked and a new access/refresh token pair is issued.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await authService.RefreshAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Ok(result);
    }

    /// <summary>Revokes a refresh token. Does not invalidate any still-live access token (it will simply expire naturally).</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await authService.LogoutAsync(request, ct);
        return NoContent();
    }

    /// <summary>Returns the authenticated user's profile, roles, and the hotel ids they may operate against.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await authService.GetCurrentUserAsync(userId, ct);
        return Ok(result);
    }
}
