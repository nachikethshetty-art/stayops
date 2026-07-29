using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace StayOps.Tests.Integration;

/// <summary>
/// Boots the real API (real EF Core migrations, real stored procedures, real seeded demo data)
/// against the same LocalDB database used for local development - see root README "Running
/// tests" for the prerequisite (run scripts/setup-database.ps1 first). Tests create their own
/// fresh rows via unique GUIDs/idempotency keys rather than depending on mutating shared seed
/// data, so they are safe to run repeatedly against the same database.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "StayOps-India-Demo-Signing-Key-Do-Not-Use-In-Production-2026!"
            });
        });
    }
}
