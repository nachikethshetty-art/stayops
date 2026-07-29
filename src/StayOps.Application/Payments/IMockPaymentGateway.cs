namespace StayOps.Application.Payments;

public enum MockGatewayStatus
{
    Succeeded,
    SentToGateway,
    Failed
}

public record MockGatewayResult(MockGatewayStatus Status, string GatewayReference);

/// <summary>
/// Mock/sandbox payment adapter. This is explicitly NOT a certified payment-gateway integration -
/// see README limitations. It exists so the online-booking-payment and refund workflows can be
/// exercised end to end without a real PCI-scoped provider.
/// </summary>
public interface IMockPaymentGateway
{
    /// <summary>Simulates confirming an online-booking payment hold. Idempotent on idempotencyKey.</summary>
    MockGatewayResult ConfirmPayment(string idempotencyKey, decimal amount);

    /// <summary>
    /// Simulates submitting a refund to the gateway. Returns immediately with SentToGateway;
    /// a background service (RefundGatewaySimulatorService) later "receives" the async completion
    /// callback and moves the refund to Succeeded, mirroring how a real gateway would behave.
    /// </summary>
    MockGatewayResult InitiateRefund(string idempotencyKey, decimal amount);
}
