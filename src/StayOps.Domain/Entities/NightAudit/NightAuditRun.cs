using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.NightAudit;

/// <summary>
/// One row per hotel per business date. A unique index on (HotelId, BusinessDate) plus a
/// "Running" status row acts as the exclusive per-hotel/business-date lock: a second concurrent
/// run attempt for the same hotel+date is rejected while a Running row exists (see sp_RunNightAudit).
/// </summary>
public class NightAuditRun : BaseEntity
{
    public Guid HotelId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public NightAuditRunStatus Status { get; set; } = NightAuditRunStatus.Running;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    public decimal TotalRoomRevenuePosted { get; set; }
    public decimal TotalTaxPosted { get; set; }
    public int StaysProcessed { get; set; }
    public int NoShowCount { get; set; }
    public int ExceptionCount { get; set; }

    public Guid? TriggeredByUserId { get; set; }

    public ICollection<NightAuditException> Exceptions { get; set; } = new List<NightAuditException>();
}

public class NightAuditException : BaseEntity
{
    public Guid NightAuditRunId { get; set; }
    public NightAuditRun? NightAuditRun { get; set; }

    public Guid? ReservationId { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
