using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StayOps.Application.Common;
using StayOps.Infrastructure.Identity;

namespace StayOps.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent startup seeder. Roles are always ensured to exist; full demo data (hotels, rooms,
/// rate plans, users, sample stays, etc.) is seeded by DemoDataSeeder, called from here, and is
/// itself idempotent so re-running the API never duplicates data.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        await DemoDataSeeder.SeedAsync(serviceProvider);
    }
}
