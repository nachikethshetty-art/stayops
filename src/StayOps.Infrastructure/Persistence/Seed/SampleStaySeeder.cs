using System.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Inventory;
using StayOps.Domain.Entities.Rates;

namespace StayOps.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds a handful of sample stays/finance records by calling the real stored procedures (not raw
/// INSERTs), so the demo data is guaranteed consistent with every business rule and invariant the
/// application itself enforces. Requires database/04-stored-procedures to already be applied.
/// </summary>
internal static class SampleStaySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider, Guid hotelId, IReadOnlyList<RoomType> roomTypes, IReadOnlyList<RatePlan> ratePlans,
        Guid guestId1, Guid guestId2, Guid companyId, string posOutletCode, string posApiKey)
    {
        var connectionFactory = serviceProvider.GetRequiredService<IDapperConnectionFactory>();
        using var connection = connectionFactory.CreateConnection();

        var businessDate = await connection.QuerySingleAsync<DateTime>(
            "SELECT BusinessDate FROM dbo.Hotels WHERE Id = @HotelId", new { HotelId = hotelId });

        var standardRoomTypeId = roomTypes[0].Id;
        var deluxeRoomTypeId = roomTypes[1].Id;
        var suiteRoomTypeId = roomTypes[2].Id;
        var roPublicId = ratePlans[0].Id;
        var cpPublicId = ratePlans[1].Id;
        var cpCorporateId = ratePlans[2].Id;

        // ---- Sample 1: historical stay, checked out, paid in full, invoiced -----------------------
        var res1 = await CreateReceptionReservationAsync(connection, hotelId, standardRoomTypeId, roPublicId, guestId1,
            businessDate.AddDays(-2), businessDate, adults: 1);
        var room1 = await GetAvailableRoomAsync(connection, hotelId, standardRoomTypeId);
        var folios1 = await CheckInAsync(connection, res1, room1);
        var guestFolio1 = folios1.First(f => f.FolioType == 0).FolioId;

        await PostChargeAsync(connection, guestFolio1, chargeType: 1, chargeCategory: 1, description: "Room service dinner", amount: 950m);
        var balance1 = await GetFolioBalanceAsync(connection, guestFolio1);
        await RecordPaymentAsync(connection, guestFolio1, balance1, method: 0 /* Cash */);
        await CheckOutAsync(connection, res1, force: false);
        await GenerateInvoiceAsync(connection, guestFolio1);

        // ---- Sample 2: currently checked-in stay (shows up live on the dashboard) -----------------
        var res2 = await CreateReceptionReservationAsync(connection, hotelId, deluxeRoomTypeId, cpPublicId, guestId2,
            businessDate, businessDate.AddDays(2), adults: 2);
        var room2 = await GetAvailableRoomAsync(connection, hotelId, deluxeRoomTypeId);
        await CheckInAsync(connection, res2, room2);

        // ---- Sample 3: upcoming confirmed corporate-billed reservation (not checked in yet) -------
        await CreateReceptionReservationAsync(connection, hotelId, suiteRoomTypeId, cpCorporateId, guestId1,
            businessDate.AddDays(5), businessDate.AddDays(7), adults: 2, companyId: companyId, billToCompany: true);

        // ---- Sample 4: online booking, then cancelled with a pending refund -----------------------
        var holdId = await CreateHoldAsync(connection, hotelId, standardRoomTypeId, roPublicId, businessDate.AddDays(15), businessDate.AddDays(17), guestId2);
        var res4 = await ConfirmOnlineReservationAsync(connection, holdId);
        await CancelReservationAsync(connection, res4, hoursBeforeCheckIn: 300, businessDate);

        // ---- Sample 5: a POS charge on the currently checked-in stay ------------------------------
        await PostPosChargeAsync(connection, hotelId, posOutletCode, room2RoomNumber: await GetRoomNumberAsync(connection, room2), amount: 650m);
    }

    private static async Task<Guid> CreateReceptionReservationAsync(
        IDbConnection connection, Guid hotelId, Guid roomTypeId, Guid ratePlanId, Guid guestId,
        DateTime checkIn, DateTime checkOut, int adults, Guid? companyId = null, bool billToCompany = false)
    {
        var p = new DynamicParameters();
        p.Add("HotelId", hotelId);
        p.Add("RoomTypeId", roomTypeId);
        p.Add("RatePlanId", ratePlanId);
        p.Add("CheckInDate", checkIn, DbType.Date);
        p.Add("CheckOutDate", checkOut, DbType.Date);
        p.Add("GuestId", guestId);
        p.Add("RoomsRequested", 1);
        p.Add("Adults", adults);
        p.Add("Children", 0);
        p.Add("IdempotencyKey", Guid.NewGuid().ToString());
        p.Add("CompanyId", companyId);
        p.Add("BillRoomChargeToCompany", billToCompany);

        var row = await connection.QuerySingleAsync(
            new CommandDefinition("sp_CreateReservation", p, commandType: CommandType.StoredProcedure));
        return (Guid)row.Id;
    }

    private static async Task<Guid> CreateHoldAsync(IDbConnection connection, Guid hotelId, Guid roomTypeId, Guid ratePlanId, DateTime checkIn, DateTime checkOut, Guid guestId)
    {
        var p = new DynamicParameters();
        p.Add("HotelId", hotelId);
        p.Add("RoomTypeId", roomTypeId);
        p.Add("RatePlanId", ratePlanId);
        p.Add("CheckInDate", checkIn, DbType.Date);
        p.Add("CheckOutDate", checkOut, DbType.Date);
        p.Add("RoomsRequested", 1);
        p.Add("Adults", 2);
        p.Add("Children", 0);
        p.Add("Source", 0 /* OnlineDirect */);
        p.Add("IdempotencyKey", Guid.NewGuid().ToString());
        p.Add("GuestId", guestId);

        var row = await connection.QuerySingleAsync(
            new CommandDefinition("sp_CreateInventoryHold", p, commandType: CommandType.StoredProcedure));
        return (Guid)row.HoldId;
    }

    private static async Task<Guid> ConfirmOnlineReservationAsync(IDbConnection connection, Guid holdId)
    {
        var p = new DynamicParameters();
        p.Add("HoldId", holdId);
        p.Add("IdempotencyKey", Guid.NewGuid().ToString());
        p.Add("PaymentReference", "MOCK-PAY-SEED");

        var row = await connection.QuerySingleAsync(
            new CommandDefinition("sp_ConfirmOnlineReservation", p, commandType: CommandType.StoredProcedure));
        return (Guid)row.Id;
    }

    private static async Task CancelReservationAsync(IDbConnection connection, Guid reservationId, int hoursBeforeCheckIn, DateTime businessDate)
    {
        var p = new DynamicParameters();
        p.Add("ReservationId", reservationId);
        p.Add("TriggerType", 0 /* GuestCancellation */);
        p.Add("HoursBeforeCheckIn", hoursBeforeCheckIn);
        p.Add("BusinessDate", businessDate, DbType.Date);
        p.Add("Reason", "Demo seed data - plans changed");

        await connection.ExecuteAsync(new CommandDefinition("sp_CancelReservation", p, commandType: CommandType.StoredProcedure));
    }

    private static async Task<Guid> GetAvailableRoomAsync(IDbConnection connection, Guid hotelId, Guid roomTypeId)
    {
        return await connection.QuerySingleAsync<Guid>(
            "SELECT TOP 1 Id FROM dbo.Rooms WHERE HotelId = @HotelId AND RoomTypeId = @RoomTypeId AND Status = 0 ORDER BY RoomNumber",
            new { HotelId = hotelId, RoomTypeId = roomTypeId });
    }

    private static async Task<string> GetRoomNumberAsync(IDbConnection connection, Guid roomId)
        => await connection.QuerySingleAsync<string>("SELECT RoomNumber FROM dbo.Rooms WHERE Id = @RoomId", new { RoomId = roomId });

    private static async Task<IReadOnlyList<(int FolioType, Guid FolioId)>> CheckInAsync(IDbConnection connection, Guid reservationId, Guid roomId)
    {
        var p = new DynamicParameters();
        p.Add("ReservationId", reservationId);
        p.Add("RoomId", roomId);

        var rows = await connection.QueryAsync(new CommandDefinition("sp_CheckInGuest", p, commandType: CommandType.StoredProcedure));
        return rows.Select(r => ((int)r.FolioType, (Guid)r.FolioId)).ToList();
    }

    private static async Task CheckOutAsync(IDbConnection connection, Guid reservationId, bool force)
    {
        var p = new DynamicParameters();
        p.Add("ReservationId", reservationId);
        p.Add("ForceCheckout", force);
        await connection.ExecuteAsync(new CommandDefinition("sp_CheckOutGuest", p, commandType: CommandType.StoredProcedure));
    }

    private static async Task PostChargeAsync(IDbConnection connection, Guid folioId, int chargeType, int chargeCategory, string description, decimal amount)
    {
        var p = new DynamicParameters();
        p.Add("FolioId", folioId);
        p.Add("ChargeType", chargeType);
        p.Add("ChargeCategory", chargeCategory);
        p.Add("Description", description);
        p.Add("TaxableAmount", amount);
        await connection.ExecuteAsync(new CommandDefinition("sp_PostFolioCharge", p, commandType: CommandType.StoredProcedure));
    }

    private static async Task<decimal> GetFolioBalanceAsync(IDbConnection connection, Guid folioId)
        => await connection.QuerySingleAsync<decimal>("SELECT Balance FROM dbo.Folios WHERE Id = @FolioId", new { FolioId = folioId });

    private static async Task RecordPaymentAsync(IDbConnection connection, Guid folioId, decimal amount, int method)
    {
        if (amount <= 0) return;
        var p = new DynamicParameters();
        p.Add("FolioId", folioId);
        p.Add("Amount", amount);
        p.Add("Method", method);
        p.Add("IdempotencyKey", Guid.NewGuid().ToString());
        await connection.ExecuteAsync(new CommandDefinition("sp_RecordFolioPayment", p, commandType: CommandType.StoredProcedure));
    }

    private static async Task GenerateInvoiceAsync(IDbConnection connection, Guid folioId)
    {
        var p = new DynamicParameters();
        p.Add("FolioId", folioId);
        await connection.ExecuteAsync(new CommandDefinition("sp_GenerateGstInvoice", p, commandType: CommandType.StoredProcedure));
    }

    private static async Task PostPosChargeAsync(IDbConnection connection, Guid hotelId, string outletCode, string room2RoomNumber, decimal amount)
    {
        var p = new DynamicParameters();
        p.Add("HotelId", hotelId);
        p.Add("OutletCode", outletCode);
        p.Add("PosReferenceNumber", $"SEED-{Guid.NewGuid():N}"[..16]);
        p.Add("RoomNumber", room2RoomNumber);
        p.Add("Amount", amount);
        p.Add("Description", "Breakfast buffet (seed data)");
        p.Add("ChargeCategory", 1 /* FoodAndBeverage */);
        await connection.ExecuteAsync(new CommandDefinition("sp_PostPosChargeToFolio", p, commandType: CommandType.StoredProcedure));
    }
}
