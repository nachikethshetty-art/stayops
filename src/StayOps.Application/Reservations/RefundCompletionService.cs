using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Enums;

namespace StayOps.Application.Reservations;

public class RefundCompletionService(IApplicationDbContext db) : IRefundCompletionService
{
    public async Task<IReadOnlyList<Guid>> GetPendingGatewaySettlementsAsync(TimeSpan minAge, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow - minAge;
        return await db.Refunds
            .Where(r => r.Status == RefundStatus.SentToGateway && r.SentToGatewayAtUtc != null && r.SentToGatewayAtUtc <= cutoff)
            .Select(r => r.Id)
            .ToListAsync(ct);
    }

    public async Task CompleteRefundAsync(Guid refundId, bool succeeded, string? failureReason, CancellationToken ct)
    {
        var refund = await db.Refunds.FirstOrDefaultAsync(r => r.Id == refundId, ct);
        if (refund is null || refund.Status != RefundStatus.SentToGateway) return;

        refund.Status = succeeded ? RefundStatus.Succeeded : RefundStatus.Failed;
        refund.CompletedAtUtc = DateTime.UtcNow;
        refund.FailureReason = succeeded ? null : failureReason;
        refund.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
