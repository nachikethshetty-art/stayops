using StayOps.Domain.Enums;

namespace StayOps.Application.Reservations;

public interface IRefundService
{
    Task<IReadOnlyList<RefundDto>> GetByHotelAsync(Guid hotelId, RefundStatus? status, CancellationToken ct = default);
    Task<RefundDto> GetByIdAsync(Guid refundId, CancellationToken ct = default);

    /// <summary>FinanceUser approves a requested refund; this immediately submits it to the mock gateway (Approved -> SentToGateway in one step for demo simplicity).</summary>
    Task<RefundDto> ApproveAndSendToGatewayAsync(Guid refundId, Guid? approvedByUserId, CancellationToken ct = default);

    Task<RefundDto> MarkFailedAsync(Guid refundId, string failureReason, CancellationToken ct = default);
}
