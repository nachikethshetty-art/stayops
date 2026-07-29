using FluentValidation;

namespace StayOps.Application.Hotels;

public class CreateHotelGroupRequestValidator : AbstractValidator<CreateHotelGroupRequest>
{
    public CreateHotelGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateHotelGroupRequestValidator : AbstractValidator<UpdateHotelGroupRequest>
{
    public UpdateHotelGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateHotelRequestValidator : AbstractValidator<CreateHotelRequest>
{
    public CreateHotelRequestValidator()
    {
        RuleFor(x => x.HotelGroupId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StateCode).NotEmpty().Length(2).Matches("^[0-9]{2}$").WithMessage("StateCode must be the 2-digit GST state code, e.g. '27'.");
        RuleFor(x => x.Gstin).NotEmpty().Length(15);
        RuleFor(x => x.TimeZoneId).NotEmpty();
    }
}

public class UpdateHotelRequestValidator : AbstractValidator<UpdateHotelRequest>
{
    public UpdateHotelRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StateCode).NotEmpty().Length(2).Matches("^[0-9]{2}$");
        RuleFor(x => x.Gstin).NotEmpty().Length(15);
        RuleFor(x => x.TimeZoneId).NotEmpty();
    }
}
