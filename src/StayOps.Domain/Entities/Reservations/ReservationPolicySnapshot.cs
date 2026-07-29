using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.Reservations;

/// <summary>
/// Immutable copy of the cancellation policy in force at booking time, so later edits to the
/// live CancellationPolicy/Rules never change the terms a guest already booked under.
/// </summary>
public class ReservationPolicySnapshot : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public Guid SourceCancellationPolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;

    public ICollection<ReservationPolicySnapshotRule> Rules { get; set; } = new List<ReservationPolicySnapshotRule>();
}

public class ReservationPolicySnapshotRule : BaseEntity
{
    public Guid ReservationPolicySnapshotId { get; set; }
    public ReservationPolicySnapshot? ReservationPolicySnapshot { get; set; }

    public int? HoursBeforeCheckInMin { get; set; }
    public int? HoursBeforeCheckInMax { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public decimal? PenaltyValue { get; set; }
    public bool AppliesToNoShow { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; } = string.Empty;
}
