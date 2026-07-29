using System.Data;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Pos;

namespace StayOps.Infrastructure.Pos;

internal class PosChargeRow
{
    public Guid Id { get; set; }
    public Guid FolioId { get; set; }
    public Guid FolioTransactionId { get; set; }
    public decimal Amount { get; set; }
    public decimal FolioBalance { get; set; }
    public bool WasDuplicate { get; set; }
}

public class PosChargeService(IDapperConnectionFactory connectionFactory) : IPosChargeService
{
    public async Task<PosChargeResultDto> PostChargeAsync(string apiKey, PostPosChargeRequest request, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var storedHash = await connection.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                "SELECT ApiKeyHash FROM dbo.PosOutlets WHERE HotelId = @HotelId AND Code = @Code AND IsActive = 1",
                new { HotelId = request.HotelId, Code = request.OutletCode }, cancellationToken: ct));

        if (storedHash is null || storedHash != HashApiKey(apiKey))
        {
            throw new ForbiddenAccessException("Invalid POS outlet API key.");
        }

        var parameters = new DynamicParameters();
        parameters.Add("HotelId", request.HotelId);
        parameters.Add("OutletCode", request.OutletCode);
        parameters.Add("PosReferenceNumber", request.PosReferenceNumber);
        parameters.Add("RoomNumber", request.RoomNumber);
        parameters.Add("Amount", request.Amount);
        parameters.Add("Description", request.Description);
        parameters.Add("ChargeCategory", (int)request.ChargeCategory);

        try
        {
            var command = new CommandDefinition("sp_PostPosChargeToFolio", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<PosChargeRow>(command);
            return new PosChargeResultDto(row.Id, row.FolioId, row.FolioTransactionId, row.Amount, row.FolioBalance, row.WasDuplicate);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    private static string HashApiKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes);
    }
}
