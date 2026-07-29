using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using StayOps.Application.Common.Settings;
using StayOps.Infrastructure.Identity;

namespace StayOps.Tests.Unit;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int accessTokenMinutes = 15) => new(Options.Create(new JwtSettings
    {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        SigningKey = "unit-test-signing-key-at-least-32-characters-long!",
        AccessTokenMinutes = accessTokenMinutes
    }));

    [Fact]
    public void GenerateAccessToken_IncludesUserIdRolesAndHotelClaims()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();

        var result = service.GenerateAccessToken(userId, "jdoe", "Jane Doe", ["HotelManager"], [hotelId]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        jwt.Subject.Should().Be(userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "HotelManager");
        jwt.Claims.Should().Contain(c => c.Type == "hotel_id" && c.Value == hotelId.ToString());
        jwt.Issuer.Should().Be("TestIssuer");
    }

    [Fact]
    public void GenerateAccessToken_ExpiryMatchesConfiguredMinutes()
    {
        var service = CreateService(accessTokenMinutes: 5);
        var before = DateTime.UtcNow;

        var result = service.GenerateAccessToken(Guid.NewGuid(), "jdoe", "Jane Doe", [], []);

        result.AccessTokenExpiresAtUtc.Should().BeCloseTo(before.AddMinutes(5), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void HashToken_IsDeterministic_AndDoesNotReturnRawInput()
    {
        var service = CreateService();
        var raw = "some-refresh-token-value";

        var hash1 = service.HashToken(raw);
        var hash2 = service.HashToken(raw);

        hash1.Should().Be(hash2);
        hash1.Should().NotBe(raw);
    }

    [Fact]
    public void GenerateRefreshToken_ProducesUniqueValuesEachCall()
    {
        var service = CreateService();

        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();

        token1.Should().NotBe(token2);
    }
}
