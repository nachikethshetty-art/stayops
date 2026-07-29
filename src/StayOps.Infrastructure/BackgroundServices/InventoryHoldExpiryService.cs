using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StayOps.Application.Common.Interfaces;

namespace StayOps.Infrastructure.BackgroundServices;

/// <summary>
/// Runs sp_ExpireInventoryHolds on a fixed interval so PendingPayment holds that were never
/// confirmed release their room-type inventory back to sellable stock after 10 minutes.
/// </summary>
public class InventoryHoldExpiryService(IServiceScopeFactory scopeFactory, ILogger<InventoryHoldExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var connectionFactory = scope.ServiceProvider.GetRequiredService<IDapperConnectionFactory>();
                using var connection = connectionFactory.CreateConnection();
                var expiredCount = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition("sp_ExpireInventoryHolds", commandType: System.Data.CommandType.StoredProcedure,
                        cancellationToken: stoppingToken));

                if (expiredCount > 0)
                {
                    logger.LogInformation("Inventory hold expiry sweep released {Count} expired hold(s).", expiredCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Inventory hold expiry sweep failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
