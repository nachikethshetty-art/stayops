using FluentValidation;

namespace StayOps.Application.Guests;

public class CreateGuestRequestValidator : AbstractValidator<CreateGuestRequest>
{
    public CreateGuestRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.IdProofType).NotEmpty();
        RuleFor(x => x.IdProofNumber).NotEmpty();
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.Gstin).Length(15).When(x => !string.IsNullOrEmpty(x.Gstin));
    }
}

public class UpdateGuestRequestValidator : AbstractValidator<UpdateGuestRequest>
{
    public UpdateGuestRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.Gstin).Length(15).When(x => !string.IsNullOrEmpty(x.Gstin));
    }
}
