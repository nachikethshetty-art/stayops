namespace StayOps.Application.Reservations;

public record PaymentWebhookRequest(Guid HoldId, string IdempotencyKey, decimal Amount, Guid? GuestId, bool BillRoomChargeToCompany);

/// <summary>
/// The (mock) payment-gateway webhook target. Idempotent on IdempotencyKey end to end: both the
/// mock gateway call and the underlying sp_ConfirmOnlineReservation call are safe to retry.
/// </summary>
public interface IPaymentWebhookService
{
    Task<ReservationDto> HandlePaymentConfirmedAsync(PaymentWebhookRequest request, CancellationToken ct = default);
}
