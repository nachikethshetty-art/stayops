using StayOps.Domain.Enums;

namespace StayOps.Application.Billing;

public record FolioDto(Guid Id, Guid ReservationId, FolioType Type, Guid? OwnerCompanyId, FolioStatus Status, decimal Balance, DateTime OpenedAtUtc, DateTime? ClosedAtUtc);

public record FolioTransactionDto(
    Guid Id, Guid FolioId, FolioTransactionType Type, string Description, decimal Amount, decimal GstAmount, decimal TotalAmount,
    Guid? ReversalOfTransactionId, DateOnly BusinessDate, Guid? PostedByUserId, string? SourceReference, DateTime CreatedAtUtc);

public record PostChargeRequest(FolioTransactionType ChargeType, GstChargeCategory ChargeCategory, string Description, decimal TaxableAmount);

public record TransferChargeRequest(Guid SourceTransactionId, Guid DestinationFolioId, string Reason);

public record RecordPaymentRequest(decimal Amount, PaymentMethod Method, string? GatewayReference, string? IdempotencyKey);

public record PaymentDto(Guid Id, Guid ReservationId, Guid? FolioId, decimal Amount, PaymentMethod Method, PaymentStatus Status, string? GatewayReference, DateTime CreatedAtUtc);

public record InvoiceDto(
    Guid Id, Guid ReservationId, Guid FolioId, string InvoiceNumber, DateOnly InvoiceDate,
    string SupplierGstin, string SupplierStateCode, string? BilledPartyName, string? BilledPartyGstin,
    string BilledPartyStateCode, string PlaceOfSupplyStateCode, bool IsInterState,
    decimal TotalTaxableValue, decimal TotalCgst, decimal TotalSgst, decimal TotalIgst, decimal TotalAmount);

public record InvoiceLineDto(
    Guid Id, Guid InvoiceId, string Description, string HsnSac, decimal TaxableValue,
    decimal CgstRate, decimal CgstAmount, decimal SgstRate, decimal SgstAmount, decimal IgstRate, decimal IgstAmount, decimal LineTotal);
