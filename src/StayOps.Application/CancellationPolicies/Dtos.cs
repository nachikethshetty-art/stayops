using StayOps.Domain.Enums;

namespace StayOps.Application.CancellationPolicies;

public record CancellationPolicyDto(Guid Id, Guid HotelId, string Name, bool IsActive, IReadOnlyList<CancellationPolicyRuleDto> Rules);

public record CancellationPolicyRuleDto(
    Guid Id, int? HoursBeforeCheckInMin, int? HoursBeforeCheckInMax, PenaltyType PenaltyType,
    decimal? PenaltyValue, bool AppliesToNoShow, int SortOrder, string Description);

public record CreateCancellationPolicyRequest(string Name);
public record UpdateCancellationPolicyRequest(string Name, bool IsActive);

public record UpsertCancellationPolicyRuleRequest(
    int? HoursBeforeCheckInMin, int? HoursBeforeCheckInMax, PenaltyType PenaltyType,
    decimal? PenaltyValue, bool AppliesToNoShow, int SortOrder, string Description);
