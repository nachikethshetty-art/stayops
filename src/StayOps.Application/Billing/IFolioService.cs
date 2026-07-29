namespace StayOps.Application.Billing;

public interface IFolioService
{
    Task<IReadOnlyList<FolioDto>> GetByReservationAsync(Guid reservationId, CancellationToken ct = default);
    Task<IReadOnlyList<FolioTransactionDto>> GetTransactionsAsync(Guid folioId, CancellationToken ct = default);

    Task<FolioTransactionDto> PostChargeAsync(Guid folioId, PostChargeRequest request, Guid? userId, CancellationToken ct = default);
    Task TransferChargeAsync(TransferChargeRequest request, Guid? userId, CancellationToken ct = default);
    Task<PaymentDto> RecordPaymentAsync(Guid folioId, RecordPaymentRequest request, Guid? userId, CancellationToken ct = default);

    Task<InvoiceDto> GenerateInvoiceAsync(Guid folioId, Guid? userId, CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesByReservationAsync(Guid reservationId, CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceLineDto>> GetInvoiceLinesAsync(Guid invoiceId, CancellationToken ct = default);
}
