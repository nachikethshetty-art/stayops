using FluentValidation;

namespace StayOps.Application.Corporate;

public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Gstin).NotEmpty().Length(15);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Gstin).NotEmpty().Length(15);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}

public class CreateCorporateRateContractRequestValidator : AbstractValidator<CreateCorporateRateContractRequest>
{
    public CreateCorporateRateContractRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.RatePlanId).NotEmpty();
        RuleFor(x => x.ContractEnd).GreaterThanOrEqualTo(x => x.ContractStart);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100).When(x => x.DiscountPercent.HasValue);
    }
}

public class UpdateCorporateRateContractRequestValidator : AbstractValidator<UpdateCorporateRateContractRequest>
{
    public UpdateCorporateRateContractRequestValidator()
    {
        RuleFor(x => x.ContractEnd).GreaterThanOrEqualTo(x => x.ContractStart);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100).When(x => x.DiscountPercent.HasValue);
    }
}

public class CreateTravelAgentRequestValidator : AbstractValidator<CreateTravelAgentRequest>
{
    public CreateTravelAgentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Gstin).NotEmpty().Length(15);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.CommissionPercent).InclusiveBetween(0, 100);
    }
}

public class UpdateTravelAgentRequestValidator : AbstractValidator<UpdateTravelAgentRequest>
{
    public UpdateTravelAgentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Gstin).NotEmpty().Length(15);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.CommissionPercent).InclusiveBetween(0, 100);
    }
}

public class CreateAgentRateContractRequestValidator : AbstractValidator<CreateAgentRateContractRequest>
{
    public CreateAgentRateContractRequestValidator()
    {
        RuleFor(x => x.TravelAgentId).NotEmpty();
        RuleFor(x => x.RatePlanId).NotEmpty();
        RuleFor(x => x.ContractEnd).GreaterThanOrEqualTo(x => x.ContractStart);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100).When(x => x.DiscountPercent.HasValue);
    }
}

public class UpdateAgentRateContractRequestValidator : AbstractValidator<UpdateAgentRateContractRequest>
{
    public UpdateAgentRateContractRequestValidator()
    {
        RuleFor(x => x.ContractEnd).GreaterThanOrEqualTo(x => x.ContractStart);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100).When(x => x.DiscountPercent.HasValue);
    }
}
