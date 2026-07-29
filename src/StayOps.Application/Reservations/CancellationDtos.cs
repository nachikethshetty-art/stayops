using StayOps.Domain.Enums;

namespace StayOps.Application.Reservations;

public record CancelReservationRequest(string? Reason);

public record CancellationDto(
    Guid Id, Guid ReservationId, CancellationTriggerType TriggerType, DateTime CancelledAtUtc,
    DateOnly HotelBusinessDateAtCancellation, int HoursBeforeCheckIn,
    decimal StayGrossAmount, decimal PenaltyAmount, decimal PenaltyGstAmount, decimal RefundDueAmount,
    string Reason, Guid? RefundId, RefundStatus? RefundStatus);

public record RefundDto(
    Guid Id, Guid CancellationId, Guid ReservationId, decimal Amount, RefundStatus Status,
    string? GatewayReference, string? FailureReason,
    DateTime RequestedAtUtc, DateTime? ApprovedAtUtc, DateTime? SentToGatewayAtUtc, DateTime? CompletedAtUtc);
