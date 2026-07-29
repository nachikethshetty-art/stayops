using StayOps.Domain.Enums;

namespace StayOps.Application.GstRules;

public record GstRuleDto(
    Guid Id, Guid? HotelId, GstChargeCategory ChargeCategory, string HsnSac,
    decimal? MinAmount, decimal? MaxAmount, decimal CgstRate, decimal SgstRate, decimal IgstRate,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo, bool IsActive);

public record CreateGstRuleRequest(
    GstChargeCategory ChargeCategory, string HsnSac, decimal? MinAmount, decimal? MaxAmount,
    decimal CgstRate, decimal SgstRate, decimal IgstRate, DateOnly EffectiveFrom, DateOnly? EffectiveTo, bool HotelSpecific);

public record UpdateGstRuleRequest(
    decimal? MinAmount, decimal? MaxAmount, decimal CgstRate, decimal SgstRate, decimal IgstRate,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo, bool IsActive);
