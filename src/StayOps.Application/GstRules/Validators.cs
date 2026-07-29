using FluentValidation;

namespace StayOps.Application.GstRules;

public class CreateGstRuleRequestValidator : AbstractValidator<CreateGstRuleRequest>
{
    public CreateGstRuleRequestValidator()
    {
        RuleFor(x => x.HsnSac).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CgstRate).InclusiveBetween(0, 100);
        RuleFor(x => x.SgstRate).InclusiveBetween(0, 100);
        RuleFor(x => x.IgstRate).InclusiveBetween(0, 100);
    }
}

public class UpdateGstRuleRequestValidator : AbstractValidator<UpdateGstRuleRequest>
{
    public UpdateGstRuleRequestValidator()
    {
        RuleFor(x => x.CgstRate).InclusiveBetween(0, 100);
        RuleFor(x => x.SgstRate).InclusiveBetween(0, 100);
        RuleFor(x => x.IgstRate).InclusiveBetween(0, 100);
    }
}
