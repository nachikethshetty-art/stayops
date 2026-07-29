namespace StayOps.Application.CancellationPolicies;

public interface ICancellationPolicyService
{
    Task<IReadOnlyList<CancellationPolicyDto>> GetByHotelAsync(Guid hotelId, CancellationToken ct = default);
    Task<CancellationPolicyDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default);
    Task<CancellationPolicyDto> CreateAsync(Guid hotelId, CreateCancellationPolicyRequest request, CancellationToken ct = default);
    Task<CancellationPolicyDto> UpdateAsync(Guid hotelId, Guid id, UpdateCancellationPolicyRequest request, CancellationToken ct = default);
    Task<CancellationPolicyDto> AddRuleAsync(Guid hotelId, Guid policyId, UpsertCancellationPolicyRuleRequest request, CancellationToken ct = default);
    Task<CancellationPolicyDto> UpdateRuleAsync(Guid hotelId, Guid policyId, Guid ruleId, UpsertCancellationPolicyRuleRequest request, CancellationToken ct = default);
    Task DeleteRuleAsync(Guid hotelId, Guid policyId, Guid ruleId, CancellationToken ct = default);
}
