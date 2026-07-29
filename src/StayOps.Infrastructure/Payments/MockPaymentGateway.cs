using StayOps.Application.Payments;

namespace StayOps.Infrastructure.Payments;

/// <summary>
/// Deterministic sandbox implementation: payments always succeed synchronously (demo simplicity),
/// refunds always go SentToGateway first and are "completed" asynchronously by
/// RefundGatewaySimulatorService a short delay later, so the app's refund-status polling/UI has
/// something real to observe. No network calls, no real money movement, no PCI scope.
/// </summary>
public class MockPaymentGateway : IMockPaymentGateway
{
    public MockGatewayResult ConfirmPayment(string idempotencyKey, decimal amount)
        => new(MockGatewayStatus.Succeeded, $"MOCK-PAY-{idempotencyKey}");

    public MockGatewayResult InitiateRefund(string idempotencyKey, decimal amount)
        => new(MockGatewayStatus.SentToGateway, $"MOCK-REFUND-{idempotencyKey}");
}
