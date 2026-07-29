using FluentValidation;

namespace StayOps.Application.Reservations;

public class AvailabilitySearchRequestValidator : AbstractValidator<AvailabilitySearchRequest>
{
    public AvailabilitySearchRequestValidator()
    {
        RuleFor(x => x.HotelId).NotEmpty();
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate);
        RuleFor(x => x.CheckInDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date));
        RuleFor(x => x.Adults).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Children).GreaterThanOrEqualTo(0);
    }
}

public class CreateHoldRequestValidator : AbstractValidator<CreateHoldRequest>
{
    public CreateHoldRequestValidator()
    {
        RuleFor(x => x.HotelId).NotEmpty();
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.RatePlanId).NotEmpty();
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate);
        RuleFor(x => x.RoomsRequested).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Adults).GreaterThanOrEqualTo(1);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}

public class ConfirmReservationRequestValidator : AbstractValidator<ConfirmReservationRequest>
{
    public ConfirmReservationRequestValidator()
    {
        RuleFor(x => x.HoldId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}

public class CreateReceptionReservationRequestValidator : AbstractValidator<CreateReceptionReservationRequest>
{
    public CreateReceptionReservationRequestValidator()
    {
        RuleFor(x => x.HotelId).NotEmpty();
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.RatePlanId).NotEmpty();
        RuleFor(x => x.GuestId).NotEmpty();
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate);
        RuleFor(x => x.RoomsRequested).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Adults).GreaterThanOrEqualTo(1);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}
