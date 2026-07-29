using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StayOps.Application.Auth;

namespace StayOps.Tests.Integration;

public class AuthApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuthApiTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidDemoCredentials_ReturnsTokenAndUserProfile()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("superadmin", TestAuthHelper.DemoPassword));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.User.UserName.Should().Be("superadmin");
        token.User.IsSuperAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("superadmin", "WrongPassword!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HotelManager_ScopedUser_OnlySeesOwnHotel()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory, "manager.mumbai");

        var response = await client.GetAsync("/api/v1/hotels");
        response.EnsureSuccessStatusCode();

        var hotels = await response.Content.ReadFromJsonAsync<List<HotelResponseDto>>();
        hotels.Should().HaveCount(1);
        hotels![0].Code.Should().Be("MUM01");
    }

    private record HotelResponseDto(string Code);

    [Fact]
    public async Task RefreshToken_Reuse_IsRejected()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("reception.mumbai", TestAuthHelper.DemoPassword));
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var firstRefresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshRequest(token!.RefreshToken));
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondRefresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshRequest(token.RefreshToken));
        secondRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
