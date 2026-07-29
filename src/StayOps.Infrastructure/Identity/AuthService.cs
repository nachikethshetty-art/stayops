using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StayOps.Application.Auth;
using StayOps.Application.Common;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Common.Settings;
using StayOps.Domain.Entities.Identity;

namespace StayOps.Infrastructure.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext db,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<TokenResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var user = await userManager.FindByNameAsync(request.UserNameOrEmail)
            ?? await userManager.FindByEmailAsync(request.UserNameOrEmail);

        if (user is null || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new InvalidCredentialsException();
        }

        return await IssueTokensAsync(user, ipAddress, ct);
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = jwtTokenService.HashToken(request.RefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (existing is null || !existing.IsActive)
        {
            throw new InvalidCredentialsException("Refresh token is invalid, expired, or has already been used.");
        }

        var user = await userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        var response = await IssueTokensAsync(user, ipAddress, ct);

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = jwtTokenService.HashToken(response.RefreshToken);
        await db.SaveChangesAsync(ct);

        return response;
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        var tokenHash = jwtTokenService.HashToken(request.RefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (existing is not null && existing.RevokedAtUtc is null)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        var roles = await userManager.GetRolesAsync(user);
        var hotelIds = await GetAccessibleHotelIdsAsync(user.Id, roles, ct);

        return new CurrentUserDto(
            user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, user.FullName,
            roles.ToList(), hotelIds, roles.Contains(Roles.SuperAdmin));
    }

    private async Task<TokenResponse> IssueTokensAsync(ApplicationUser user, string? ipAddress, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var hotelIds = await GetAccessibleHotelIdsAsync(user.Id, roles, ct);

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.UserName ?? string.Empty, user.FullName, roles, hotelIds);
        var rawRefreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = jwtTokenService.HashToken(rawRefreshToken),
            ExpiresAtUtc = refreshExpiresAtUtc,
            CreatedByIp = ipAddress
        });
        await db.SaveChangesAsync(ct);

        var currentUser = new CurrentUserDto(
            user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, user.FullName,
            roles.ToList(), hotelIds, roles.Contains(Roles.SuperAdmin));

        return new TokenResponse(accessToken.AccessToken, accessToken.AccessTokenExpiresAtUtc, rawRefreshToken, refreshExpiresAtUtc, currentUser);
    }

    private async Task<IReadOnlyList<Guid>> GetAccessibleHotelIdsAsync(Guid userId, IList<string> roles, CancellationToken ct)
    {
        if (roles.Contains(Roles.SuperAdmin))
        {
            return await db.Hotels.Select(h => h.Id).ToListAsync(ct);
        }

        return await db.UserHotelAccesses.Where(a => a.UserId == userId).Select(a => a.HotelId).ToListAsync(ct);
    }
}
