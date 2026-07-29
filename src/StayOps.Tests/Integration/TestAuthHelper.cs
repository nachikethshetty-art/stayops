using System.Net.Http.Headers;
using System.Net.Http.Json;
using StayOps.Application.Auth;

namespace StayOps.Tests.Integration;

public static class TestAuthHelper
{
    public const string DemoPassword = "Passw0rd!123";

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(ApiWebApplicationFactory factory, string userName, string password = DemoPassword)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(userName, password));
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(TestJson.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }
}
