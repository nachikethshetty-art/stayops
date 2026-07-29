namespace StayOps.Application.Rates;

public interface IRatePlanService
{
    Task<IReadOnlyList<RatePlanDto>> GetByHotelAsync(Guid hotelId, CancellationToken ct = default);
    Task<RatePlanDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default);
    Task<RatePlanDto> CreateAsync(Guid hotelId, CreateRatePlanRequest request, CancellationToken ct = default);
    Task<RatePlanDto> UpdateAsync(Guid hotelId, Guid id, UpdateRatePlanRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<RatePlanPriceDto>> GetPricesAsync(Guid hotelId, Guid ratePlanId, CancellationToken ct = default);
    Task<RatePlanPriceDto> AddPriceAsync(Guid hotelId, Guid ratePlanId, CreateRatePlanPriceRequest request, CancellationToken ct = default);
    Task<RatePlanPriceDto> UpdatePriceAsync(Guid hotelId, Guid ratePlanId, Guid priceId, UpdateRatePlanPriceRequest request, CancellationToken ct = default);
    Task DeletePriceAsync(Guid hotelId, Guid ratePlanId, Guid priceId, CancellationToken ct = default);
}
