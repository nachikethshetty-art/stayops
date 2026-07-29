using StayOps.Domain.Enums;

namespace StayOps.Application.Rates;

public record RatePlanDto(Guid Id, Guid HotelId, string Code, string Name, MealPlanType MealPlan, RatePlanScope Scope, Guid? CancellationPolicyId, bool IsActive);

public record CreateRatePlanRequest(string Code, string Name, MealPlanType MealPlan, RatePlanScope Scope, Guid? CancellationPolicyId);
public record UpdateRatePlanRequest(string Name, MealPlanType MealPlan, RatePlanScope Scope, Guid? CancellationPolicyId, bool IsActive);

public record RatePlanPriceDto(Guid Id, Guid RatePlanId, Guid RoomTypeId, int Occupancy, DayOfWeek? DayOfWeek, DateOnly EffectiveFrom, DateOnly EffectiveTo, decimal Rate);

public record CreateRatePlanPriceRequest(Guid RoomTypeId, int Occupancy, DayOfWeek? DayOfWeek, DateOnly EffectiveFrom, DateOnly EffectiveTo, decimal Rate);
public record UpdateRatePlanPriceRequest(int Occupancy, DayOfWeek? DayOfWeek, DateOnly EffectiveFrom, DateOnly EffectiveTo, decimal Rate);
