using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Billing;

namespace StayOps.Application.GstRules;

public class GstRuleService(IApplicationDbContext db) : IGstRuleService
{
    public async Task<IReadOnlyList<GstRuleDto>> GetForHotelAsync(Guid hotelId, CancellationToken ct = default)
    {
        return await db.GstRules
            .Where(r => r.HotelId == hotelId || r.HotelId == null)
            .OrderBy(r => r.ChargeCategory).ThenBy(r => r.MinAmount)
            .Select(ToDtoExpression)
            .ToListAsync(ct);
    }

    public async Task<GstRuleDto> CreateAsync(Guid hotelId, CreateGstRuleRequest request, bool allowGlobal, CancellationToken ct = default)
    {
        if (!allowGlobal && !request.HotelSpecific)
        {
            throw new ForbiddenAccessException("Only SuperAdmin can create a global GST rule.");
        }

        var rule = new GstRule
        {
            HotelId = request.HotelSpecific ? hotelId : null,
            ChargeCategory = request.ChargeCategory,
            HsnSac = request.HsnSac,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            CgstRate = request.CgstRate,
            SgstRate = request.SgstRate,
            IgstRate = request.IgstRate,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };
        db.GstRules.Add(rule);
        await db.SaveChangesAsync(ct);

        return await db.GstRules.Where(r => r.Id == rule.Id).Select(ToDtoExpression).FirstAsync(ct);
    }

    public async Task<GstRuleDto> UpdateAsync(Guid id, UpdateGstRuleRequest request, CancellationToken ct = default)
    {
        var rule = await db.GstRules.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException(nameof(GstRule), id);

        rule.MinAmount = request.MinAmount;
        rule.MaxAmount = request.MaxAmount;
        rule.CgstRate = request.CgstRate;
        rule.SgstRate = request.SgstRate;
        rule.IgstRate = request.IgstRate;
        rule.EffectiveFrom = request.EffectiveFrom;
        rule.EffectiveTo = request.EffectiveTo;
        rule.IsActive = request.IsActive;
        rule.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return await db.GstRules.Where(r => r.Id == id).Select(ToDtoExpression).FirstAsync(ct);
    }

    private static readonly System.Linq.Expressions.Expression<Func<GstRule, GstRuleDto>> ToDtoExpression = r => new GstRuleDto(
        r.Id, r.HotelId, r.ChargeCategory, r.HsnSac, r.MinAmount, r.MaxAmount, r.CgstRate, r.SgstRate, r.IgstRate, r.EffectiveFrom, r.EffectiveTo, r.IsActive);
}
