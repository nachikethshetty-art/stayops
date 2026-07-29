using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Domain.Enums;
using StayOps.Application.Reservations;

namespace StayOps.Api.Controllers;

/// <summary>
/// Guest-facing endpoints for the online booking demo screen - no login required, mirroring a
/// real hotel website. Both routes are idempotent via the caller-supplied IdempotencyKey.
/// </summary>
[ApiController]
[Route("api/v1/online")]
[AllowAnonymous]
public class OnlineBookingController(IReservationService reservationService, IPaymentWebhookService paymentWebhookService) : ControllerBase
{
    /// <summary>Creates a 10-minute inventory hold for a room type. Source is always forced to OnlineDirect here.</summary>
    [HttpPost("holds")]
    public async Task<ActionResult<InventoryHoldDto>> CreateHold([FromBody] CreateHoldRequest request, CancellationToken ct)
    {
        var onlineRequest = request with { Source = BookingSource.OnlineDirect };
        var result = await reservationService.CreateHoldAsync(onlineRequest, createdByUserId: null, ct);
        return Ok(result);
    }

    /// <summary>Mock payment-gateway webhook target: converts a paid, still-active hold into a Confirmed reservation.</summary>
    [HttpPost("payments/webhook")]
    public async Task<ActionResult<ReservationDto>> PaymentWebhook([FromBody] PaymentWebhookRequest request, CancellationToken ct)
    {
        var result = await paymentWebhookService.HandlePaymentConfirmedAsync(request, ct);
        return Ok(result);
    }
}
