using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Rates;

namespace StayOps.Application.Rates;

public class RatePlanService(IApplicationDbContext db) : IRatePlanService
{
    public async Task<IReadOnlyList<RatePlanDto>> GetByHotelAsync(Guid hotelId, CancellationToken ct = default)
    {
        return await db.RatePlans.Where(rp => rp.HotelId == hotelId).Select(ToDtoExpression).ToListAsync(ct);
    }

    public async Task<RatePlanDto> GetByIdAsync(Guid hotelId, Guid id, CancellationToken ct = default)
    {
        return await db.RatePlans.Where(rp => rp.HotelId == hotelId && rp.Id == id).Select(ToDtoExpression).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(RatePlan), id);
    }

    public async Task<RatePlanDto> CreateAsync(Guid hotelId, CreateRatePlanRequest request, CancellationToken ct = default)
    {
        if (await db.RatePlans.AnyAsync(rp => rp.HotelId == hotelId && rp.Code == request.Code, ct))
        {
            throw new ConflictException($"Rate plan code '{request.Code}' already exists for this hotel.");
        }

        var ratePlan = new RatePlan
        {
            HotelId = hotelId,
            Code = request.Code,
            Name = request.Name,
            MealPlan = request.MealPlan,
            Scope = request.Scope,
            CancellationPolicyId = request.CancellationPolicyId
        };
        db.RatePlans.Add(ratePlan);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, ratePlan.Id, ct);
    }

    public async Task<RatePlanDto> UpdateAsync(Guid hotelId, Guid id, UpdateRatePlanRequest request, CancellationToken ct = default)
    {
        var ratePlan = await db.RatePlans.FirstOrDefaultAsync(rp => rp.HotelId == hotelId && rp.Id == id, ct)
            ?? throw new NotFoundException(nameof(RatePlan), id);

        ratePlan.Name = request.Name;
        ratePlan.MealPlan = request.MealPlan;
        ratePlan.Scope = request.Scope;
        ratePlan.CancellationPolicyId = request.CancellationPolicyId;
        ratePlan.IsActive = request.IsActive;
        ratePlan.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(hotelId, id, ct);
    }

    public async Task<IReadOnlyList<RatePlanPriceDto>> GetPricesAsync(Guid hotelId, Guid ratePlanId, CancellationToken ct = default)
    {
        await EnsureRatePlanBelongsToHotel(hotelId, ratePlanId, ct);

        return await db.RatePlanPrices
            .Where(p => p.RatePlanId == ratePlanId)
            .OrderBy(p => p.RoomTypeId).ThenBy(p => p.Occupancy).ThenBy(p => p.EffectiveFrom)
            .Select(ToPriceDtoExpression)
            .ToListAsync(ct);
    }

    public async Task<RatePlanPriceDto> AddPriceAsync(Guid hotelId, Guid ratePlanId, CreateRatePlanPriceRequest request, CancellationToken ct = default)
    {
        await EnsureRatePlanBelongsToHotel(hotelId, ratePlanId, ct);

        var price = new RatePlanPrice
        {
            RatePlanId = ratePlanId,
            RoomTypeId = request.RoomTypeId,
            Occupancy = request.Occupancy,
            DayOfWeek = request.DayOfWeek,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Rate = request.Rate
        };
        db.RatePlanPrices.Add(price);
        await db.SaveChangesAsync(ct);

        return await db.RatePlanPrices.Where(p => p.Id == price.Id).Select(ToPriceDtoExpression).FirstAsync(ct);
    }

    public async Task<RatePlanPriceDto> UpdatePriceAsync(Guid hotelId, Guid ratePlanId, Guid priceId, UpdateRatePlanPriceRequest request, CancellationToken ct = default)
    {
        await EnsureRatePlanBelongsToHotel(hotelId, ratePlanId, ct);

        var price = await db.RatePlanPrices.FirstOrDefaultAsync(p => p.RatePlanId == ratePlanId && p.Id == priceId, ct)
            ?? throw new NotFoundException(nameof(RatePlanPrice), priceId);

        price.Occupancy = request.Occupancy;
        price.DayOfWeek = request.DayOfWeek;
        price.EffectiveFrom = request.EffectiveFrom;
        price.EffectiveTo = request.EffectiveTo;
        price.Rate = request.Rate;
        price.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await db.RatePlanPrices.Where(p => p.Id == priceId).Select(ToPriceDtoExpression).FirstAsync(ct);
    }

    public async Task DeletePriceAsync(Guid hotelId, Guid ratePlanId, Guid priceId, CancellationToken ct = default)
    {
        await EnsureRatePlanBelongsToHotel(hotelId, ratePlanId, ct);

        var price = await db.RatePlanPrices.FirstOrDefaultAsync(p => p.RatePlanId == ratePlanId && p.Id == priceId, ct)
            ?? throw new NotFoundException(nameof(RatePlanPrice), priceId);

        db.RatePlanPrices.Remove(price);
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureRatePlanBelongsToHotel(Guid hotelId, Guid ratePlanId, CancellationToken ct)
    {
        var exists = await db.RatePlans.AnyAsync(rp => rp.Id == ratePlanId && rp.HotelId == hotelId, ct);
        if (!exists) throw new NotFoundException(nameof(RatePlan), ratePlanId);
    }

    private static readonly System.Linq.Expressions.Expression<Func<RatePlan, RatePlanDto>> ToDtoExpression = rp => new RatePlanDto(
        rp.Id, rp.HotelId, rp.Code, rp.Name, rp.MealPlan, rp.Scope, rp.CancellationPolicyId, rp.IsActive);

    private static readonly System.Linq.Expressions.Expression<Func<RatePlanPrice, RatePlanPriceDto>> ToPriceDtoExpression = p => new RatePlanPriceDto(
        p.Id, p.RatePlanId, p.RoomTypeId, p.Occupancy, p.DayOfWeek, p.EffectiveFrom, p.EffectiveTo, p.Rate);
}
