namespace StayOps.Application.Reservations;

/// <summary>Finalizes a refund whose mock-gateway call has "completed" asynchronously (see RefundGatewaySimulatorService).</summary>
public interface IRefundCompletionService
{
    Task<IReadOnlyList<Guid>> GetPendingGatewaySettlementsAsync(TimeSpan minAge, CancellationToken ct);
    Task CompleteRefundAsync(Guid refundId, bool succeeded, string? failureReason, CancellationToken ct);
}
