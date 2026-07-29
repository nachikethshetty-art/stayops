using System.Data;
using Dapper;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Reservations;
using StayOps.Domain.Enums;

namespace StayOps.Infrastructure.Reservations;

public class ReservationService(IDapperConnectionFactory connectionFactory) : IReservationService
{
    public async Task<InventoryHoldDto> CreateHoldAsync(CreateHoldRequest request, Guid? createdByUserId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("HotelId", request.HotelId);
        parameters.Add("RoomTypeId", request.RoomTypeId);
        parameters.Add("RatePlanId", request.RatePlanId);
        parameters.Add("CheckInDate", request.CheckInDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("CheckOutDate", request.CheckOutDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("RoomsRequested", request.RoomsRequested);
        parameters.Add("Adults", request.Adults);
        parameters.Add("Children", request.Children);
        parameters.Add("Source", (int)request.Source);
        parameters.Add("IdempotencyKey", request.IdempotencyKey);
        parameters.Add("GuestId", request.GuestId);
        parameters.Add("CompanyId", request.CompanyId);
        parameters.Add("TravelAgentId", request.TravelAgentId);
        parameters.Add("CreatedByUserId", createdByUserId);

        try
        {
            var command = new CommandDefinition("sp_CreateInventoryHold", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<InventoryHoldRow>(command);
            return ToDto(row);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<ReservationDto> ConfirmAsync(ConfirmReservationRequest request, Guid? createdByUserId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("HoldId", request.HoldId);
        parameters.Add("IdempotencyKey", request.IdempotencyKey);
        parameters.Add("PaymentReference", request.PaymentReference);
        parameters.Add("GuestId", request.GuestId);
        parameters.Add("BillRoomChargeToCompany", request.BillRoomChargeToCompany);
        parameters.Add("CreatedByUserId", createdByUserId);

        try
        {
            var command = new CommandDefinition("sp_ConfirmOnlineReservation", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<ReservationRow>(command);
            return ToDto(row);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<ReservationDto> CreateReceptionReservationAsync(CreateReceptionReservationRequest request, Guid? createdByUserId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("HotelId", request.HotelId);
        parameters.Add("RoomTypeId", request.RoomTypeId);
        parameters.Add("RatePlanId", request.RatePlanId);
        parameters.Add("CheckInDate", request.CheckInDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("CheckOutDate", request.CheckOutDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("GuestId", request.GuestId);
        parameters.Add("RoomsRequested", request.RoomsRequested);
        parameters.Add("Adults", request.Adults);
        parameters.Add("Children", request.Children);
        parameters.Add("IdempotencyKey", request.IdempotencyKey);
        parameters.Add("CompanyId", request.CompanyId);
        parameters.Add("TravelAgentId", request.TravelAgentId);
        parameters.Add("BillRoomChargeToCompany", request.BillRoomChargeToCompany);
        parameters.Add("CreatedByUserId", createdByUserId);

        try
        {
            var command = new CommandDefinition("sp_CreateReservation", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<ReservationRow>(command);
            return ToDto(row);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<ReservationDto?> GetByIdAsync(Guid reservationId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT * FROM dbo.Reservations WHERE Id = @Id", new { Id = reservationId }, cancellationToken: ct);
        var row = await connection.QuerySingleOrDefaultAsync<ReservationRow>(command);
        return row is null ? null : ToDto(row);
    }

    public async Task<IReadOnlyList<ReservationListItemDto>> GetByHotelAsync(Guid hotelId, DateOnly? checkInDate, DateOnly? checkOutDate, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
            SELECT ReservationId, HotelId, ReservationNumber, Status, Source, CheckInDate, CheckOutDate,
                   RoomsBooked, Adults, Children, GuestId, GuestName, GuestPhone, GuestEmail,
                   RoomTypeId, RoomTypeName, RatePlanId, RatePlanName, CompanyId, CompanyName, CreatedAtUtc
            FROM dbo.vw_ReservationSummary
            WHERE HotelId = @HotelId
              AND (@CheckInDate IS NULL OR CheckInDate >= @CheckInDate)
              AND (@CheckOutDate IS NULL OR CheckOutDate <= @CheckOutDate)
            ORDER BY CheckInDate DESC
            """;

        var command = new CommandDefinition(sql, new
        {
            HotelId = hotelId,
            CheckInDate = checkInDate.HasValue ? checkInDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
            CheckOutDate = checkOutDate.HasValue ? checkOutDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
        }, cancellationToken: ct);

        var rows = await connection.QueryAsync<ReservationListItemRow>(command);
        return rows.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<ReservationNightRateDto>> GetNightRatesAsync(Guid reservationId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT StayDate, RoomRate, MealPlan, CgstRate, SgstRate, IgstRate
            FROM dbo.ReservationNightRates
            WHERE ReservationId = @ReservationId
            ORDER BY StayDate
            """;
        var command = new CommandDefinition(sql, new { ReservationId = reservationId }, cancellationToken: ct);
        var rows = await connection.QueryAsync<ReservationNightRateRow>(command);
        return rows.Select(r => new ReservationNightRateDto(
            DateOnly.FromDateTime(r.StayDate), r.RoomRate, (MealPlanType)r.MealPlan, r.CgstRate, r.SgstRate, r.IgstRate)).ToList();
    }

    private static InventoryHoldDto ToDto(InventoryHoldRow r) => new(
        r.HoldId, r.HotelId, r.RoomTypeId, r.RatePlanId, DateOnly.FromDateTime(r.CheckInDate), DateOnly.FromDateTime(r.CheckOutDate),
        r.RoomsRequested, (InventoryHoldStatus)r.Status, (BookingSource)r.Source, r.ExpiresAtUtc,
        r.GuestId, r.CompanyId, r.TravelAgentId, r.ReservationId);

    private static ReservationDto ToDto(ReservationRow r) => new(
        r.Id, r.HotelId, r.ReservationNumber, r.GuestId, r.CompanyId, r.TravelAgentId, r.RoomTypeId, r.RatePlanId,
        DateOnly.FromDateTime(r.CheckInDate), DateOnly.FromDateTime(r.CheckOutDate), r.RoomsBooked, r.Adults, r.Children,
        (ReservationStatus)r.Status, (BookingSource)r.Source, r.InventoryHoldId, r.IdempotencyKey,
        DateOnly.FromDateTime(r.BusinessDateCreated), r.CreatedByUserId, r.BillRoomChargeToCompany, r.CreatedAtUtc, r.UpdatedAtUtc);

    private static ReservationListItemDto ToDto(ReservationListItemRow r) => new(
        r.ReservationId, r.HotelId, r.ReservationNumber, (ReservationStatus)r.Status, (BookingSource)r.Source,
        DateOnly.FromDateTime(r.CheckInDate), DateOnly.FromDateTime(r.CheckOutDate), r.RoomsBooked, r.Adults, r.Children,
        r.GuestId, r.GuestName, r.GuestPhone, r.GuestEmail, r.RoomTypeId, r.RoomTypeName, r.RatePlanId, r.RatePlanName,
        r.CompanyId, r.CompanyName, r.CreatedAtUtc);
}
