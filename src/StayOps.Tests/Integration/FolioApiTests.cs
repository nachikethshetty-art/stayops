using System.Net.Http.Json;
using FluentAssertions;
using StayOps.Application.Billing;
using StayOps.Application.Common.Models;
using StayOps.Application.Hotels;
using StayOps.Application.Inventory;
using StayOps.Application.Rates;
using StayOps.Application.Reservations;
using StayOps.Domain.Enums;

namespace StayOps.Tests.Integration;

public class FolioApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public FolioApiTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private record GuestResponseDto(Guid Id);

    [Fact]
    public async Task PostChargeThenPay_ClosesGuestFolioBalanceToZero_AndInvoiceMatchesTotals()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory, "manager.mumbai");

        var hotels = await client.GetFromJsonAsync<List<HotelDto>>("/api/v1/hotels", TestJson.Options);
        var mumbai = hotels!.Single(h => h.Code == "MUM01");

        var roomTypes = await client.GetFromJsonAsync<List<RoomTypeDto>>($"/api/v1/hotels/{mumbai.Id}/room-types", TestJson.Options);
        var standard = roomTypes!.Single(rt => rt.Code == "STD");

        var ratePlans = await client.GetFromJsonAsync<List<RatePlanDto>>($"/api/v1/hotels/{mumbai.Id}/rate-plans", TestJson.Options);
        var roPublic = ratePlans!.Single(rp => rp.Code == "RO-PUB");

        var hotelDetail = await client.GetFromJsonAsync<HotelDto>($"/api/v1/hotels/{mumbai.Id}", TestJson.Options);
        var checkIn = hotelDetail!.BusinessDate; // today's business date so check-in is immediately allowed
        var checkOut = checkIn.AddDays(1);

        var guests = await client.GetFromJsonAsync<PagedResult<GuestResponseDto>>("/api/v1/guests?page=1&pageSize=1", TestJson.Options);
        var guestId = guests!.Items[0].Id;

        var reservationRequest = new
        {
            hotelId = mumbai.Id,
            roomTypeId = standard.Id,
            ratePlanId = roPublic.Id,
            checkInDate = checkIn,
            checkOutDate = checkOut,
            guestId,
            roomsRequested = 1,
            adults = 1,
            children = 0,
            idempotencyKey = Guid.NewGuid().ToString(),
            billRoomChargeToCompany = false
        };
        var reservationResponse = await client.PostAsJsonAsync($"/api/v1/hotels/{mumbai.Id}/reservations", reservationRequest, TestJson.Options);
        reservationResponse.EnsureSuccessStatusCode();
        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationDto>(TestJson.Options);

        var availableRooms = await client.GetFromJsonAsync<List<RoomDto>>(
            $"/api/v1/hotels/{mumbai.Id}/rooms?roomTypeId={standard.Id}&status=Available", TestJson.Options);
        var room = availableRooms!.First();

        var checkInResponse = await client.PostAsJsonAsync($"/api/v1/reservations/{reservation!.Id}/check-in", new { roomId = room.Id }, TestJson.Options);
        checkInResponse.EnsureSuccessStatusCode();
        var folios = await checkInResponse.Content.ReadFromJsonAsync<List<StayFolioSummaryDto>>(TestJson.Options);
        var guestFolio = folios!.Single(f => f.FolioType == FolioType.Guest);

        var chargeResponse = await client.PostAsJsonAsync($"/api/v1/folios/{guestFolio.FolioId}/charges", new
        {
            chargeType = FolioTransactionType.Incidental,
            chargeCategory = GstChargeCategory.FoodAndBeverage,
            description = "Test snack charge",
            taxableAmount = 400m
        }, TestJson.Options);
        chargeResponse.EnsureSuccessStatusCode();
        var charge = await chargeResponse.Content.ReadFromJsonAsync<FolioTransactionDto>(TestJson.Options);

        // 400 taxable + 5% GST (2.5 CGST + 2.5 SGST in the demo slabs) = 420 total.
        charge!.TotalAmount.Should().Be(420m);

        var paymentResponse = await client.PostAsJsonAsync($"/api/v1/folios/{guestFolio.FolioId}/payments", new
        {
            amount = 420m,
            method = PaymentMethod.Cash,
            idempotencyKey = Guid.NewGuid().ToString()
        }, TestJson.Options);
        paymentResponse.EnsureSuccessStatusCode();

        var folioAfterPayment = await client.GetFromJsonAsync<List<FolioDto>>($"/api/v1/reservations/{reservation.Id}/folios", TestJson.Options);
        folioAfterPayment!.Single(f => f.Type == FolioType.Guest).Balance.Should().Be(0m);

        var invoiceResponse = await client.PostAsJsonAsync($"/api/v1/folios/{guestFolio.FolioId}/invoices", new { }, TestJson.Options);
        invoiceResponse.EnsureSuccessStatusCode();
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>(TestJson.Options);

        invoice!.TotalTaxableValue.Should().Be(400m);
        invoice.TotalAmount.Should().Be(420m);
    }

    private record StayFolioSummaryDto(Guid FolioId, FolioType FolioType, FolioStatus FolioStatus, decimal Balance);
}
