using StayOps.Application.Common.Exceptions;
using StayOps.Application.Payments;

namespace StayOps.Application.Reservations;

public class PaymentWebhookService(IMockPaymentGateway gateway, IReservationService reservationService) : IPaymentWebhookService
{
    public async Task<ReservationDto> HandlePaymentConfirmedAsync(PaymentWebhookRequest request, CancellationToken ct = default)
    {
        var gatewayResult = gateway.ConfirmPayment(request.IdempotencyKey, request.Amount);
        if (gatewayResult.Status != MockGatewayStatus.Succeeded)
        {
            throw new BusinessRuleException("Mock payment gateway did not confirm this payment.");
        }

        var confirmRequest = new ConfirmReservationRequest(
            request.HoldId, request.IdempotencyKey, gatewayResult.GatewayReference, request.GuestId, request.BillRoomChargeToCompany);

        return await reservationService.ConfirmAsync(confirmRequest, createdByUserId: null, ct);
    }
}
