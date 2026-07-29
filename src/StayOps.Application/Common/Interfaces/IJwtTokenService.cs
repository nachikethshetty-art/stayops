namespace StayOps.Application.Common.Interfaces;

public record JwtTokenResult(string AccessToken, DateTime AccessTokenExpiresAtUtc);

public interface IJwtTokenService
{
    JwtTokenResult GenerateAccessToken(Guid userId, string userName, string fullName, IEnumerable<string> roles, IEnumerable<Guid> accessibleHotelIds);

    /// <summary>Generates a cryptographically random opaque refresh token string (not a JWT).</summary>
    string GenerateRefreshToken();

    string HashToken(string rawToken);
}
