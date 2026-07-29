namespace StayOps.Application.GstRules;

public interface IGstRuleService
{
    /// <summary>Returns both hotel-specific rules and the global (HotelId == null) defaults, so the admin screen can show the full effective picture.</summary>
    Task<IReadOnlyList<GstRuleDto>> GetForHotelAsync(Guid hotelId, CancellationToken ct = default);
    Task<GstRuleDto> CreateAsync(Guid hotelId, CreateGstRuleRequest request, bool allowGlobal, CancellationToken ct = default);
    Task<GstRuleDto> UpdateAsync(Guid id, UpdateGstRuleRequest request, CancellationToken ct = default);
}
