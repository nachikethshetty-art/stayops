using System.Net.Http.Json;
using FluentAssertions;
using StayOps.Application.Common.Models;
using StayOps.Application.Hotels;
using StayOps.Application.Inventory;
using StayOps.Application.Rates;
using StayOps.Application.Reservations;

namespace StayOps.Tests.Integration;

public class CancellationApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public CancellationApiTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private record GuestResponseDto(Guid Id);

    private async Task<(HttpClient client, ReservationDto reservation)> CreateReservationAsync(int daysUntilCheckIn)
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory, "reception.mumbai");

        var hotels = await client.GetFromJsonAsync<List<HotelDto>>("/api/v1/hotels", TestJson.Options);
        var mumbai = hotels!.Single(h => h.Code == "MUM01");
        var roomTypes = await client.GetFromJsonAsync<List<RoomTypeDto>>($"/api/v1/hotels/{mumbai.Id}/room-types", TestJson.Options);
        var standard = roomTypes!.Single(rt => rt.Code == "STD");
        var ratePlans = await client.GetFromJsonAsync<List<RatePlanDto>>($"/api/v1/hotels/{mumbai.Id}/rate-plans", TestJson.Options);
        var roPublic = ratePlans!.Single(rp => rp.Code == "RO-PUB");
        var guests = await client.GetFromJsonAsync<PagedResult<GuestResponseDto>>("/api/v1/guests?page=1&pageSize=1", TestJson.Options);
        var guestId = guests!.Items[0].Id;

        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysUntilCheckIn));
        var request = new
        {
            hotelId = mumbai.Id,
            roomTypeId = standard.Id,
            ratePlanId = roPublic.Id,
            checkInDate = checkIn,
            checkOutDate = checkIn.AddDays(2),
            guestId,
            roomsRequested = 1,
            adults = 1,
            children = 0,
            idempotencyKey = Guid.NewGuid().ToString(),
            billRoomChargeToCompany = false
        };

        var response = await client.PostAsJsonAsync($"/api/v1/hotels/{mumbai.Id}/reservations", request, TestJson.Options);
        response.EnsureSuccessStatusCode();
        var reservation = (await response.Content.ReadFromJsonAsync<ReservationDto>(TestJson.Options))!;
        return (client, reservation);
    }

    [Fact]
    public async Task Cancel_MoreThanSevenDaysBeforeCheckIn_HasNoPenalty()
    {
        var (client, reservation) = await CreateReservationAsync(daysUntilCheckIn: 20);

        var response = await client.PostAsJsonAsync($"/api/v1/reservations/{reservation.Id}/cancel", new { reason = "Plans changed" }, TestJson.Options);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CancellationDto>(TestJson.Options);
        result!.PenaltyAmount.Should().Be(0m);
        result.PenaltyGstAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Cancel_LessThan24HoursBeforeCheckIn_ChargesFullStayPenalty()
    {
        var (client, reservation) = await CreateReservationAsync(daysUntilCheckIn: 0);

        var response = await client.PostAsJsonAsync($"/api/v1/reservations/{reservation.Id}/cancel", new { reason = "Last minute" }, TestJson.Options);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CancellationDto>(TestJson.Options);
        result!.PenaltyAmount.Should().Be(result.StayGrossAmount);
        result.RefundDueAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Cancel_Twice_IsIdempotent_AndReturnsSameCancellationRecord()
    {
        var (client, reservation) = await CreateReservationAsync(daysUntilCheckIn: 20);

        var firstResponse = await client.PostAsJsonAsync($"/api/v1/reservations/{reservation.Id}/cancel", new { reason = "First call" }, TestJson.Options);
        var firstResult = await firstResponse.Content.ReadFromJsonAsync<CancellationDto>(TestJson.Options);

        var secondResponse = await client.PostAsJsonAsync($"/api/v1/reservations/{reservation.Id}/cancel", new { reason = "Retry" }, TestJson.Options);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<CancellationDto>(TestJson.Options);

        secondResult!.Id.Should().Be(firstResult!.Id);
    }
}
