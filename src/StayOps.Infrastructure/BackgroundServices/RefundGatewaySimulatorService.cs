using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StayOps.Application.Reservations;

namespace StayOps.Infrastructure.BackgroundServices;

/// <summary>
/// Simulates the asynchronous leg of a payment-gateway refund: any Refund sitting in
/// SentToGateway for more than 15 seconds is "completed" here, mirroring a real gateway's
/// async webhook/callback without any actual network call (mock adapter, see IMockPaymentGateway).
/// </summary>
public class RefundGatewaySimulatorService(IServiceScopeFactory scopeFactory, ILogger<RefundGatewaySimulatorService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan SettlementDelay = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var refundService = scope.ServiceProvider.GetRequiredService<IRefundCompletionService>();

                var pending = await refundService.GetPendingGatewaySettlementsAsync(SettlementDelay, stoppingToken);
                foreach (var refundId in pending)
                {
                    await refundService.CompleteRefundAsync(refundId, succeeded: true, failureReason: null, stoppingToken);
                    logger.LogInformation("Simulated gateway settlement completed for refund {RefundId}.", refundId);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Refund gateway simulator sweep failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
