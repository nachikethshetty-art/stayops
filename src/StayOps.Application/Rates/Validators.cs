using FluentValidation;

namespace StayOps.Application.Rates;

public class CreateRatePlanRequestValidator : AbstractValidator<CreateRatePlanRequest>
{
    public CreateRatePlanRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MealPlan).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();
    }
}

public class UpdateRatePlanRequestValidator : AbstractValidator<UpdateRatePlanRequest>
{
    public UpdateRatePlanRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MealPlan).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();
    }
}

public class CreateRatePlanPriceRequestValidator : AbstractValidator<CreateRatePlanPriceRequest>
{
    public CreateRatePlanPriceRequestValidator()
    {
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.Occupancy).GreaterThanOrEqualTo(1);
        RuleFor(x => x.EffectiveTo).GreaterThanOrEqualTo(x => x.EffectiveFrom);
        RuleFor(x => x.Rate).GreaterThan(0);
    }
}

public class UpdateRatePlanPriceRequestValidator : AbstractValidator<UpdateRatePlanPriceRequest>
{
    public UpdateRatePlanPriceRequestValidator()
    {
        RuleFor(x => x.Occupancy).GreaterThanOrEqualTo(1);
        RuleFor(x => x.EffectiveTo).GreaterThanOrEqualTo(x => x.EffectiveFrom);
        RuleFor(x => x.Rate).GreaterThan(0);
    }
}
