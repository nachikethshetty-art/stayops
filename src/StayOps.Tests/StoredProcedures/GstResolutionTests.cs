using Dapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StayOps.Application.Common.Interfaces;
using StayOps.Tests.Integration;

namespace StayOps.Tests.StoredProcedures;

/// <summary>Exercises the GST tariff-slab resolution function directly against SQL Server, independent of any API/HTTP layer.</summary>
public class GstResolutionTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public GstResolutionTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(500, 0, 0, 0)]      // below the nil slab threshold
    [InlineData(3500, 6, 6, 12)]    // mid slab
    [InlineData(8500, 9, 9, 18)]    // top slab
    public async Task FnResolveRoomTariffGst_PicksCorrectSlab_ForRoomRate(decimal amount, decimal expectedCgst, decimal expectedSgst, decimal expectedIgst)
    {
        using var scope = _factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDapperConnectionFactory>();
        using var connection = connectionFactory.CreateConnection();

        var hotelId = await connection.QuerySingleAsync<Guid>("SELECT TOP 1 Id FROM dbo.Hotels WHERE Code = 'MUM01'");

        var result = await connection.QuerySingleAsync<(decimal CgstRate, decimal SgstRate, decimal IgstRate)>(
            "SELECT CgstRate, SgstRate, IgstRate FROM dbo.fn_ResolveRoomTariffGst(@HotelId, @Amount, @StayDate)",
            new { HotelId = hotelId, Amount = amount, StayDate = DateTime.UtcNow.Date });

        result.CgstRate.Should().Be(expectedCgst);
        result.SgstRate.Should().Be(expectedSgst);
        result.IgstRate.Should().Be(expectedIgst);
    }

    [Fact]
    public async Task FnRoomTypeAvailableCount_NeverExceedsTotalActiveRooms()
    {
        using var scope = _factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDapperConnectionFactory>();
        using var connection = connectionFactory.CreateConnection();

        var hotelId = await connection.QuerySingleAsync<Guid>("SELECT TOP 1 Id FROM dbo.Hotels WHERE Code = 'MUM01'");
        var roomTypeId = await connection.QuerySingleAsync<Guid>(
            "SELECT TOP 1 Id FROM dbo.RoomTypes WHERE HotelId = @HotelId AND Code = 'STD'", new { HotelId = hotelId });
        var totalRooms = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM dbo.Rooms WHERE HotelId = @HotelId AND RoomTypeId = @RoomTypeId AND IsActive = 1",
            new { HotelId = hotelId, RoomTypeId = roomTypeId });

        // A far-future date with no bookings should show full (or at most full) availability.
        var checkIn = DateTime.UtcNow.Date.AddDays(900);
        var checkOut = checkIn.AddDays(1);

        var available = await connection.QuerySingleAsync<int>(
            "SELECT dbo.fn_RoomTypeAvailableCount(@HotelId, @RoomTypeId, @CheckIn, @CheckOut)",
            new { HotelId = hotelId, RoomTypeId = roomTypeId, CheckIn = checkIn, CheckOut = checkOut });

        available.Should().BeLessThanOrEqualTo(totalRooms).And.BeGreaterThan(0);
    }
}
