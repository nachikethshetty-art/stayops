using System.Data;
using Dapper;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Reservations;

namespace StayOps.Infrastructure.Reservations;

public class AvailabilityService(IDapperConnectionFactory connectionFactory) : IAvailabilityService
{
    public async Task<IReadOnlyList<RoomTypeAvailabilityDto>> SearchAsync(AvailabilitySearchRequest request, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("HotelId", request.HotelId);
        parameters.Add("CheckInDate", request.CheckInDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("CheckOutDate", request.CheckOutDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("Adults", request.Adults);
        parameters.Add("Children", request.Children);
        parameters.Add("RequestedRatePlanId", request.RatePlanId);
        parameters.Add("CompanyId", request.CompanyId);
        parameters.Add("TravelAgentId", request.TravelAgentId);

        var command = new CommandDefinition(
            "sp_SearchAvailableRoomTypes", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);

        var rows = await connection.QueryAsync<RoomTypeAvailabilityDto>(command);
        return rows.ToList();
    }
}
