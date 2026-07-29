using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.CancellationPolicies;

namespace StayOps.Application.CancellationPolicies;

public class CancellationPolicyService(IApplicationDbContext db) : ICancellationPolicyService
{
    public async Task<IReadOnlyList<CancellationPolicyDto>> GetByHotelAsync(Guid hotelId, CancellationToken ct = default)
    {
        var policies = await db.CancellationPolicies.Where(p => p.HotelId == hotelId)
            .Include(p => p.Rules)
            .ToListAsync(ct);
        return policies.Select(ToDto).ToList();
    }

    public async Task<CancellationPolicyDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default)
    {
        var policy = await db.CancellationPolicies.Where(p => p.HotelId == hotelId && p.Id == id)
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(CancellationPolicy), id);
        return ToDto(policy);
    }

    public async Task<CancellationPolicyDto> CreateAsync(Guid hotelId, CreateCancellationPolicyRequest request, CancellationToken ct = default)
    {
        var policy = new CancellationPolicy { HotelId = hotelId, Name = request.Name };
        db.CancellationPolicies.Add(policy);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, policy.Id, ct);
    }

    public async Task<CancellationPolicyDto> UpdateAsync(Guid hotelId, Guid id, UpdateCancellationPolicyRequest request, CancellationToken ct = default)
    {
        var policy = await db.CancellationPolicies.FirstOrDefaultAsync(p => p.HotelId == hotelId && p.Id == id, ct)
            ?? throw new NotFoundException(nameof(CancellationPolicy), id);

        policy.Name = request.Name;
        policy.IsActive = request.IsActive;
        policy.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, id, ct);
    }

    public async Task<CancellationPolicyDto> AddRuleAsync(Guid hotelId, Guid policyId, UpsertCancellationPolicyRuleRequest request, CancellationToken ct = default)
    {
        var policyExists = await db.CancellationPolicies.AnyAsync(p => p.HotelId == hotelId && p.Id == policyId, ct);
        if (!policyExists) throw new NotFoundException(nameof(CancellationPolicy), policyId);

        db.CancellationPolicyRules.Add(new CancellationPolicyRule
        {
            CancellationPolicyId = policyId,
            HoursBeforeCheckInMin = request.HoursBeforeCheckInMin,
            HoursBeforeCheckInMax = request.HoursBeforeCheckInMax,
            PenaltyType = request.PenaltyType,
            PenaltyValue = request.PenaltyValue,
            AppliesToNoShow = request.AppliesToNoShow,
            SortOrder = request.SortOrder,
            Description = request.Description
        });
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, policyId, ct);
    }

    public async Task<CancellationPolicyDto> UpdateRuleAsync(Guid hotelId, Guid policyId, Guid ruleId, UpsertCancellationPolicyRuleRequest request, CancellationToken ct = default)
    {
        var rule = await db.CancellationPolicyRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.CancellationPolicyId == policyId, ct)
            ?? throw new NotFoundException(nameof(CancellationPolicyRule), ruleId);

        rule.HoursBeforeCheckInMin = request.HoursBeforeCheckInMin;
        rule.HoursBeforeCheckInMax = request.HoursBeforeCheckInMax;
        rule.PenaltyType = request.PenaltyType;
        rule.PenaltyValue = request.PenaltyValue;
        rule.AppliesToNoShow = request.AppliesToNoShow;
        rule.SortOrder = request.SortOrder;
        rule.Description = request.Description;
        rule.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, policyId, ct);
    }

    public async Task DeleteRuleAsync(Guid hotelId, Guid policyId, Guid ruleId, CancellationToken ct = default)
    {
        var rule = await db.CancellationPolicyRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.CancellationPolicyId == policyId, ct)
            ?? throw new NotFoundException(nameof(CancellationPolicyRule), ruleId);
        db.CancellationPolicyRules.Remove(rule);
        await db.SaveChangesAsync(ct);
    }

    private static CancellationPolicyDto ToDto(CancellationPolicy p) => new(
        p.Id, p.HotelId, p.Name, p.IsActive,
        p.Rules.OrderBy(r => r.SortOrder).Select(r => new CancellationPolicyRuleDto(
            r.Id, r.HoursBeforeCheckInMin, r.HoursBeforeCheckInMax, r.PenaltyType, r.PenaltyValue, r.AppliesToNoShow, r.SortOrder, r.Description)).ToList());
}
