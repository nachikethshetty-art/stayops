using System.Data;
using Dapper;
using StayOps.Application.Billing;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Enums;

namespace StayOps.Infrastructure.Billing;

public class FolioService(IDapperConnectionFactory connectionFactory) : IFolioService
{
    public async Task<IReadOnlyList<FolioDto>> GetByReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT Id, ReservationId, Type, OwnerCompanyId, Status, Balance, OpenedAtUtc, ClosedAtUtc FROM dbo.Folios WHERE ReservationId = @ReservationId ORDER BY Type";
        var rows = await connection.QueryAsync<FolioDto>(new CommandDefinition(sql, new { ReservationId = reservationId }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<FolioTransactionDto>> GetTransactionsAsync(Guid folioId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, FolioId, Type, Description, Amount, GstAmount, TotalAmount, ReversalOfTransactionId, BusinessDate, PostedByUserId, SourceReference, CreatedAtUtc
            FROM dbo.FolioTransactions WHERE FolioId = @FolioId ORDER BY CreatedAtUtc
            """;
        var rows = await connection.QueryAsync<FolioTransactionRow>(new CommandDefinition(sql, new { FolioId = folioId }, cancellationToken: ct));
        return rows.Select(ToDto).ToList();
    }

    public async Task<FolioTransactionDto> PostChargeAsync(Guid folioId, PostChargeRequest request, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("FolioId", folioId);
        parameters.Add("ChargeType", (int)request.ChargeType);
        parameters.Add("ChargeCategory", (int)request.ChargeCategory);
        parameters.Add("Description", request.Description);
        parameters.Add("TaxableAmount", request.TaxableAmount);
        parameters.Add("PostedByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_PostFolioCharge", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<FolioTransactionRow>(command);
            return ToDto(row);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task TransferChargeAsync(TransferChargeRequest request, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("SourceTransactionId", request.SourceTransactionId);
        parameters.Add("DestinationFolioId", request.DestinationFolioId);
        parameters.Add("Reason", request.Reason);
        parameters.Add("TransferredByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_TransferFolioCharge", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            await connection.ExecuteAsync(command);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<PaymentDto> RecordPaymentAsync(Guid folioId, RecordPaymentRequest request, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("FolioId", folioId);
        parameters.Add("Amount", request.Amount);
        parameters.Add("Method", (int)request.Method);
        parameters.Add("GatewayReference", request.GatewayReference);
        parameters.Add("IdempotencyKey", request.IdempotencyKey);
        parameters.Add("RecordedByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_RecordFolioPayment", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<dynamic>(command);
            return new PaymentDto(
                row.Id, row.ReservationId, row.FolioId, row.Amount, (PaymentMethod)row.Method, (PaymentStatus)row.Status, row.GatewayReference, row.CreatedAtUtc);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<InvoiceDto> GenerateInvoiceAsync(Guid folioId, Guid? userId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("FolioId", folioId);
        parameters.Add("CreatedByUserId", userId);

        try
        {
            var command = new CommandDefinition("sp_GenerateGstInvoice", parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct);
            var row = await connection.QuerySingleAsync<InvoiceRow>(command);
            return ToDto(row);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Class == 16)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesByReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.Invoices WHERE ReservationId = @ReservationId ORDER BY InvoiceDate";
        var rows = await connection.QueryAsync<InvoiceRow>(new CommandDefinition(sql, new { ReservationId = reservationId }, cancellationToken: ct));
        return rows.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<InvoiceLineDto>> GetInvoiceLinesAsync(Guid invoiceId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, InvoiceId, Description, HsnSac, TaxableValue, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount, LineTotal
            FROM dbo.InvoiceLines WHERE InvoiceId = @InvoiceId
            """;
        var rows = await connection.QueryAsync<InvoiceLineDto>(new CommandDefinition(sql, new { InvoiceId = invoiceId }, cancellationToken: ct));
        return rows.ToList();
    }

    private static FolioTransactionDto ToDto(FolioTransactionRow r) => new(
        r.Id, r.FolioId, (FolioTransactionType)r.Type, r.Description, r.Amount, r.GstAmount, r.TotalAmount,
        r.ReversalOfTransactionId, DateOnly.FromDateTime(r.BusinessDate), r.PostedByUserId, r.SourceReference, r.CreatedAtUtc);

    private static InvoiceDto ToDto(InvoiceRow r) => new(
        r.Id, r.ReservationId, r.FolioId, r.InvoiceNumber, DateOnly.FromDateTime(r.InvoiceDate),
        r.SupplierGstin, r.SupplierStateCode, r.BilledPartyName, r.BilledPartyGstin, r.BilledPartyStateCode,
        r.PlaceOfSupplyStateCode, r.IsInterState, r.TotalTaxableValue, r.TotalCgst, r.TotalSgst, r.TotalIgst, r.TotalAmount);
}
