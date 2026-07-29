using FluentValidation;

namespace StayOps.Application.Housekeeping;

public class CreateHousekeepingTaskRequestValidator : AbstractValidator<CreateHousekeepingTaskRequest>
{
    public CreateHousekeepingTaskRequestValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
    }
}

public class UpdateHousekeepingTaskStatusRequestValidator : AbstractValidator<UpdateHousekeepingTaskStatusRequest>
{
    public UpdateHousekeepingTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
