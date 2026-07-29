namespace StayOps.Application.NightAudit;

public interface INightAuditService
{
    Task<NightAuditRunDto> RunAsync(Guid hotelId, Guid? triggeredByUserId, CancellationToken ct = default);
    Task<IReadOnlyList<NightAuditRunDto>> GetHistoryAsync(Guid hotelId, CancellationToken ct = default);
    Task<IReadOnlyList<NightAuditExceptionDto>> GetExceptionsAsync(Guid runId, CancellationToken ct = default);
}
