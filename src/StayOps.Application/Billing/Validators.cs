using FluentValidation;

namespace StayOps.Application.Billing;

public class PostChargeRequestValidator : AbstractValidator<PostChargeRequest>
{
    public PostChargeRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TaxableAmount).GreaterThan(0);
        RuleFor(x => x.ChargeType).Must(t => t is Domain.Enums.FolioTransactionType.RoomCharge or Domain.Enums.FolioTransactionType.Incidental)
            .WithMessage("Only RoomCharge or Incidental may be posted manually.");
    }
}

public class TransferChargeRequestValidator : AbstractValidator<TransferChargeRequest>
{
    public TransferChargeRequestValidator()
    {
        RuleFor(x => x.SourceTransactionId).NotEmpty();
        RuleFor(x => x.DestinationFolioId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class RecordPaymentRequestValidator : AbstractValidator<RecordPaymentRequest>
{
    public RecordPaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
