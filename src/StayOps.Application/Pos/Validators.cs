using FluentValidation;

namespace StayOps.Application.Pos;

public class PostPosChargeRequestValidator : AbstractValidator<PostPosChargeRequest>
{
    public PostPosChargeRequestValidator()
    {
        RuleFor(x => x.HotelId).NotEmpty();
        RuleFor(x => x.OutletCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PosReferenceNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
