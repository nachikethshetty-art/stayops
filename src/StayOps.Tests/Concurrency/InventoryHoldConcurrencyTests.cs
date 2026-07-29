using System.Data;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using StayOps.Application.Common.Interfaces;
using StayOps.Tests.Integration;

namespace StayOps.Tests.Concurrency;

/// <summary>
/// Proves sp_CreateInventoryHold prevents overbooking under real concurrent load: fires more
/// simultaneous hold requests than there is physical room inventory for a room type, and asserts
/// that exactly the available count succeed while the rest are rejected - never more holds than
/// rooms. Uses a far-future date range unique to each test run so it never collides with seed
/// data or other tests' bookings.
/// </summary>
public class InventoryHoldConcurrencyTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public InventoryHoldConcurrencyTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConcurrentHoldRequests_ForLimitedInventory_NeverExceedsAvailableRooms()
    {
        using var scope = _factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDapperConnectionFactory>();

        Guid hotelId, roomTypeId, ratePlanId;
        int totalRoomsOfType;
        using (var setupConnection = connectionFactory.CreateConnection())
        {
            hotelId = await setupConnection.QuerySingleAsync<Guid>("SELECT TOP 1 Id FROM dbo.Hotels WHERE Code = 'MUM01'");
            roomTypeId = await setupConnection.QuerySingleAsync<Guid>(
                "SELECT TOP 1 Id FROM dbo.RoomTypes WHERE HotelId = @HotelId AND Code = 'STE'", new { HotelId = hotelId });
            ratePlanId = await setupConnection.QuerySingleAsync<Guid>(
                "SELECT TOP 1 Id FROM dbo.RatePlans WHERE HotelId = @HotelId AND Code = 'RO-PUB'", new { HotelId = hotelId });
            totalRoomsOfType = await setupConnection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.Rooms WHERE HotelId = @HotelId AND RoomTypeId = @RoomTypeId AND IsActive = 1",
                new { HotelId = hotelId, RoomTypeId = roomTypeId });
        }

        totalRoomsOfType.Should().BeGreaterThan(0, "the Suite room type must have seeded rooms for this test to be meaningful");

        // Far-future, unique-per-run date range so this test never collides with seed data or other test runs.
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300 + Random.Shared.Next(1, 500)));
        var checkOut = checkIn.AddDays(1);

        var concurrentAttempts = totalRoomsOfType + 5; // deliberately request more holds than rooms exist

        var tasks = Enumerable.Range(0, concurrentAttempts).Select(async _ =>
        {
            using var connection = connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("HotelId", hotelId);
            parameters.Add("RoomTypeId", roomTypeId);
            parameters.Add("RatePlanId", ratePlanId);
            parameters.Add("CheckInDate", checkIn.ToDateTime(TimeOnly.MinValue), DbType.Date);
            parameters.Add("CheckOutDate", checkOut.ToDateTime(TimeOnly.MinValue), DbType.Date);
            parameters.Add("RoomsRequested", 1);
            parameters.Add("Adults", 1);
            parameters.Add("Children", 0);
            parameters.Add("Source", 1);
            parameters.Add("IdempotencyKey", Guid.NewGuid().ToString());

            try
            {
                await connection.QuerySingleAsync(
                    new CommandDefinition("sp_CreateInventoryHold", parameters, commandType: CommandType.StoredProcedure));
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);

        results.Count(succeeded => succeeded).Should().Be(totalRoomsOfType,
            "exactly as many concurrent hold requests as there are physical rooms of this type should succeed - no more, no fewer");
    }
}
