using StayOps.Domain.Enums;

namespace StayOps.Application.NightAudit;

public record NightAuditRunDto(
    Guid Id, Guid HotelId, DateOnly BusinessDate, NightAuditRunStatus Status,
    DateTime StartedAtUtc, DateTime? CompletedAtUtc,
    decimal TotalRoomRevenuePosted, decimal TotalTaxPosted, int StaysProcessed, int NoShowCount, int ExceptionCount);

public record NightAuditExceptionDto(Guid Id, Guid? ReservationId, string ExceptionType, string Message, DateTime CreatedAtUtc);
