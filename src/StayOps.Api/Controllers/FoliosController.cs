using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Billing;
using StayOps.Application.Common;

namespace StayOps.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.Receptionist},{Roles.FinanceUser}")]
public class FoliosController(IFolioService service) : ControllerBase
{
    private Guid? CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet("api/v1/reservations/{reservationId:guid}/folios")]
    public async Task<ActionResult<IReadOnlyList<FolioDto>>> GetByReservation(Guid reservationId, CancellationToken ct)
        => Ok(await service.GetByReservationAsync(reservationId, ct));

    [HttpGet("api/v1/folios/{folioId:guid}/transactions")]
    public async Task<ActionResult<IReadOnlyList<FolioTransactionDto>>> GetTransactions(Guid folioId, CancellationToken ct)
        => Ok(await service.GetTransactionsAsync(folioId, ct));

    [HttpPost("api/v1/folios/{folioId:guid}/charges")]
    public async Task<ActionResult<FolioTransactionDto>> PostCharge(Guid folioId, [FromBody] PostChargeRequest request, CancellationToken ct)
        => Ok(await service.PostChargeAsync(folioId, request, CurrentUserId, ct));

    [HttpPost("api/v1/folios/transfers")]
    public async Task<IActionResult> TransferCharge([FromBody] TransferChargeRequest request, CancellationToken ct)
    {
        await service.TransferChargeAsync(request, CurrentUserId, ct);
        return NoContent();
    }

    [HttpPost("api/v1/folios/{folioId:guid}/payments")]
    public async Task<ActionResult<PaymentDto>> RecordPayment(Guid folioId, [FromBody] RecordPaymentRequest request, CancellationToken ct)
        => Ok(await service.RecordPaymentAsync(folioId, request, CurrentUserId, ct));

    [HttpPost("api/v1/folios/{folioId:guid}/invoices")]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
    public async Task<ActionResult<InvoiceDto>> GenerateInvoice(Guid folioId, CancellationToken ct)
        => Ok(await service.GenerateInvoiceAsync(folioId, CurrentUserId, ct));

    [HttpGet("api/v1/reservations/{reservationId:guid}/invoices")]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetInvoices(Guid reservationId, CancellationToken ct)
        => Ok(await service.GetInvoicesByReservationAsync(reservationId, ct));

    [HttpGet("api/v1/invoices/{invoiceId:guid}/lines")]
    public async Task<ActionResult<IReadOnlyList<InvoiceLineDto>>> GetInvoiceLines(Guid invoiceId, CancellationToken ct)
        => Ok(await service.GetInvoiceLinesAsync(invoiceId, ct));
}
