namespace StayOps.Application.Pos;

public interface IPosChargeService
{
    /// <summary>Validates the outlet API key belongs to (HotelId, OutletCode) before posting the charge via sp_PostPosChargeToFolio.</summary>
    Task<PosChargeResultDto> PostChargeAsync(string apiKey, PostPosChargeRequest request, CancellationToken ct = default);
}
