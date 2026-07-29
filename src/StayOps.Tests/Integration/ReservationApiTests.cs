using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StayOps.Application.Common.Models;
using StayOps.Application.Hotels;
using StayOps.Application.Inventory;
using StayOps.Application.Rates;
using StayOps.Application.Reservations;

namespace StayOps.Tests.Integration;

public class ReservationApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ReservationApiTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<(Guid hotelId, Guid roomTypeId, Guid ratePlanId)> ResolveMumbaiStandardRoomAsync(HttpClient client)
    {
        var hotels = await client.GetFromJsonAsync<List<HotelDto>>("/api/v1/hotels", TestJson.Options);
        var mumbai = hotels!.Single(h => h.Code == "MUM01");

        var roomTypes = await client.GetFromJsonAsync<List<RoomTypeDto>>($"/api/v1/hotels/{mumbai.Id}/room-types", TestJson.Options);
        var standard = roomTypes!.Single(rt => rt.Code == "STD");

        var ratePlans = await client.GetFromJsonAsync<List<RatePlanDto>>($"/api/v1/hotels/{mumbai.Id}/rate-plans", TestJson.Options);
        var roPublic = ratePlans!.Single(rp => rp.Code == "RO-PUB");

        return (mumbai.Id, standard.Id, roPublic.Id);
    }

    [Fact]
    public async Task ReceptionBooking_CreatesConfirmedReservation_AndReducesAvailability()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory, "reception.mumbai");
        var (hotelId, roomTypeId, ratePlanId) = await ResolveMumbaiStandardRoomAsync(client);

        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(40));
        var checkOut = checkIn.AddDays(2);

        var before = await client.GetFromJsonAsync<List<RoomTypeAvailabilityDto>>(
            $"/api/v1/hotels/{hotelId}/availability?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&adults=1&children=0", TestJson.Options);
        var beforeCount = before!.Single(r => r.RoomTypeId == roomTypeId).AvailableCount;

        var guests = await client.GetFromJsonAsync<PagedResult<GuestResponseDto>>("/api/v1/guests?page=1&pageSize=1", TestJson.Options);
        var guestId = guests!.Items[0].Id;

        var request = new
        {
            hotelId,
            roomTypeId,
            ratePlanId,
            checkInDate = checkIn,
            checkOutDate = checkOut,
            guestId,
            roomsRequested = 1,
            adults = 1,
            children = 0,
            idempotencyKey = Guid.NewGuid().ToString(),
            billRoomChargeToCompany = false
        };

        var response = await client.PostAsJsonAsync($"/api/v1/hotels/{hotelId}/reservations", request, TestJson.Options);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservation = await response.Content.ReadFromJsonAsync<ReservationDto>(TestJson.Options);
        reservation!.Status.Should().Be(Domain.Enums.ReservationStatus.Confirmed);

        var after = await client.GetFromJsonAsync<List<RoomTypeAvailabilityDto>>(
            $"/api/v1/hotels/{hotelId}/availability?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&adults=1&children=0", TestJson.Options);
        var afterCount = after!.Single(r => r.RoomTypeId == roomTypeId).AvailableCount;

        afterCount.Should().Be(beforeCount - 1);
    }

    [Fact]
    public async Task CreateReservation_WithSameIdempotencyKey_DoesNotDoubleBook()
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(_factory, "reception.mumbai");
        var (hotelId, roomTypeId, ratePlanId) = await ResolveMumbaiStandardRoomAsync(client);

        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60));
        var checkOut = checkIn.AddDays(1);
        var idempotencyKey = Guid.NewGuid().ToString();

        var guests = await client.GetFromJsonAsync<PagedResult<GuestResponseDto>>("/api/v1/guests?page=1&pageSize=1", TestJson.Options);
        var guestId = guests!.Items[0].Id;

        var request = new
        {
            hotelId,
            roomTypeId,
            ratePlanId,
            checkInDate = checkIn,
            checkOutDate = checkOut,
            guestId,
            roomsRequested = 1,
            adults = 1,
            children = 0,
            idempotencyKey,
            billRoomChargeToCompany = false
        };

        var first = await client.PostAsJsonAsync($"/api/v1/hotels/{hotelId}/reservations", request, TestJson.Options);
        var second = await client.PostAsJsonAsync($"/api/v1/hotels/{hotelId}/reservations", request, TestJson.Options);

        var firstReservation = await first.Content.ReadFromJsonAsync<ReservationDto>(TestJson.Options);
        var secondReservation = await second.Content.ReadFromJsonAsync<ReservationDto>(TestJson.Options);

        secondReservation!.Id.Should().Be(firstReservation!.Id, "a retried request with the same idempotency key must return the original reservation, not create a new one");
    }

    private record GuestResponseDto(Guid Id);
}
