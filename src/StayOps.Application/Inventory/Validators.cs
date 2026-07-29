using FluentValidation;

namespace StayOps.Application.Inventory;

public class CreateRoomTypeRequestValidator : AbstractValidator<CreateRoomTypeRequest>
{
    public CreateRoomTypeRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BaseOccupancy).GreaterThan(0);
        RuleFor(x => x.MaxOccupancy).GreaterThanOrEqualTo(x => x.BaseOccupancy);
        RuleFor(x => x.MaxChildren).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRoomTypeRequestValidator : AbstractValidator<UpdateRoomTypeRequest>
{
    public UpdateRoomTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BaseOccupancy).GreaterThan(0);
        RuleFor(x => x.MaxOccupancy).GreaterThanOrEqualTo(x => x.BaseOccupancy);
        RuleFor(x => x.MaxChildren).GreaterThanOrEqualTo(0);
    }
}

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(20);
    }
}

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(20);
    }
}
