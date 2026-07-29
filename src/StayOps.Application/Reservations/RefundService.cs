using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Payments;
using StayOps.Domain.Entities.Reservations;
using StayOps.Domain.Enums;

namespace StayOps.Application.Reservations;

public class RefundService(IApplicationDbContext db, IMockPaymentGateway gateway) : IRefundService
{
    public async Task<IReadOnlyList<RefundDto>> GetByHotelAsync(Guid hotelId, RefundStatus? status, CancellationToken ct = default)
    {
        var query =
            from refund in db.Refunds
            join reservation in db.Reservations on refund.ReservationId equals reservation.Id
            where reservation.HotelId == hotelId
            select refund;

        if (status is not null) query = query.Where(r => r.Status == status);

        return await query.OrderByDescending(r => r.RequestedAtUtc).Select(ToDtoExpression).ToListAsync(ct);
    }

    public async Task<RefundDto> GetByIdAsync(Guid refundId, CancellationToken ct = default)
    {
        return await db.Refunds.Where(r => r.Id == refundId).Select(ToDtoExpression).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Refund), refundId);
    }

    public async Task<RefundDto> ApproveAndSendToGatewayAsync(Guid refundId, Guid? approvedByUserId, CancellationToken ct = default)
    {
        var refund = await db.Refunds.FirstOrDefaultAsync(r => r.Id == refundId, ct)
            ?? throw new NotFoundException(nameof(Refund), refundId);

        if (refund.Status != RefundStatus.RefundRequested)
        {
            throw new BusinessRuleException($"Refund is in status '{refund.Status}' and cannot be approved from here.");
        }

        var now = DateTime.UtcNow;
        refund.Status = RefundStatus.Approved;
        refund.ApprovedAtUtc = now;
        refund.ApprovedByUserId = approvedByUserId;

        var gatewayResult = gateway.InitiateRefund(refund.Id.ToString(), refund.Amount);
        refund.Status = RefundStatus.SentToGateway;
        refund.SentToGatewayAtUtc = DateTime.UtcNow;
        refund.GatewayReference = gatewayResult.GatewayReference;
        refund.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(refundId, ct);
    }

    public async Task<RefundDto> MarkFailedAsync(Guid refundId, string failureReason, CancellationToken ct = default)
    {
        var refund = await db.Refunds.FirstOrDefaultAsync(r => r.Id == refundId, ct)
            ?? throw new NotFoundException(nameof(Refund), refundId);

        if (refund.Status is RefundStatus.Succeeded or RefundStatus.Failed)
        {
            throw new BusinessRuleException($"Refund is already in a terminal status '{refund.Status}'.");
        }

        refund.Status = RefundStatus.Failed;
        refund.FailureReason = failureReason;
        refund.CompletedAtUtc = DateTime.UtcNow;
        refund.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(refundId, ct);
    }

    private static readonly System.Linq.Expressions.Expression<Func<Refund, RefundDto>> ToDtoExpression = r => new RefundDto(
        r.Id, r.CancellationId, r.ReservationId, r.Amount, r.Status, r.GatewayReference, r.FailureReason,
        r.RequestedAtUtc, r.ApprovedAtUtc, r.SentToGatewayAtUtc, r.CompletedAtUtc);
}
