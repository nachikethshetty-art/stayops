using StayOps.Domain.Common;
using StayOps.Domain.Enums;

namespace StayOps.Domain.Entities.CancellationPolicies;

public class CancellationPolicy : BaseEntity
{
    public Guid HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<CancellationPolicyRule> Rules { get; set; } = new List<CancellationPolicyRule>();
}

/// <summary>
/// A single tier of a cancellation policy, expressed as an hours-before-checkin window.
/// Demo default policy: full refund at 168h+ (7 days), one-night penalty between 24h-168h,
/// full-stay penalty (no-show / late cancellation) under 24h.
/// Rules are evaluated by HoursBeforeCheckInMin/Max against (check-in instant - cancellation instant)
/// in the hotel's local timezone.
/// </summary>
public class CancellationPolicyRule : BaseEntity
{
    public Guid CancellationPolicyId { get; set; }
    public CancellationPolicy? CancellationPolicy { get; set; }

    /// <summary>Rule applies when hours-before-checkin &gt;= this value (inclusive). Null = no lower bound.</summary>
    public int? HoursBeforeCheckInMin { get; set; }

    /// <summary>Rule applies when hours-before-checkin &lt; this value (exclusive). Null = no upper bound.</summary>
    public int? HoursBeforeCheckInMax { get; set; }

    public PenaltyType PenaltyType { get; set; }

    /// <summary>Meaning depends on PenaltyType: null for NoPenalty/FullStayPenalty/OneNightPenalty, percentage (0-100) for PercentageOfStay.</summary>
    public decimal? PenaltyValue { get; set; }

    public bool AppliesToNoShow { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; } = string.Empty;
}
