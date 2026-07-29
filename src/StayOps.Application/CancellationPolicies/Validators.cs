using FluentValidation;

namespace StayOps.Application.CancellationPolicies;

public class CreateCancellationPolicyRequestValidator : AbstractValidator<CreateCancellationPolicyRequest>
{
    public CreateCancellationPolicyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateCancellationPolicyRequestValidator : AbstractValidator<UpdateCancellationPolicyRequest>
{
    public UpdateCancellationPolicyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpsertCancellationPolicyRuleRequestValidator : AbstractValidator<UpsertCancellationPolicyRuleRequest>
{
    public UpsertCancellationPolicyRuleRequestValidator()
    {
        RuleFor(x => x.PenaltyType).IsInEnum();
        RuleFor(x => x.PenaltyValue).InclusiveBetween(0, 100).When(x => x.PenaltyValue.HasValue);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
