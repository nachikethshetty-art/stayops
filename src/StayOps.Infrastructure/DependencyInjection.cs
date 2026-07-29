using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayOps.Application.Auth;
using StayOps.Application.Billing;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Common.Settings;
using StayOps.Application.Inventory;
using StayOps.Application.NightAudit;
using StayOps.Application.Payments;
using StayOps.Application.Pos;
using StayOps.Application.Reports;
using StayOps.Application.Reservations;
using StayOps.Infrastructure.BackgroundServices;
using StayOps.Infrastructure.Identity;
using StayOps.Infrastructure.Payments;
using StayOps.Infrastructure.Persistence;
using StayOps.Infrastructure.Reservations;

namespace StayOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDapperConnectionFactory, DapperConnectionFactory>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddSingleton<IMockPaymentGateway, MockPaymentGateway>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ICancellationService, CancellationService>();
        services.AddScoped<ICheckInCheckOutService, CheckInCheckOutService>();
        services.AddScoped<IFolioService, Billing.FolioService>();
        services.AddScoped<IPosChargeService, Pos.PosChargeService>();
        services.AddScoped<IRoomOutOfServiceService, Inventory.RoomOutOfServiceService>();
        services.AddScoped<INightAuditService, NightAudit.NightAuditService>();
        services.AddScoped<IReportService, Reports.ReportService>();

        services.AddHostedService<InventoryHoldExpiryService>();
        services.AddHostedService<RefundGatewaySimulatorService>();

        return services;
    }
}
