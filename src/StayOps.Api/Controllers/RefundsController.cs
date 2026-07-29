using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Reservations;
using StayOps.Domain.Enums;

namespace StayOps.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.HotelManager},{Roles.FinanceUser}")]
public class RefundsController(IRefundService service) : ControllerBase
{
    private Guid? CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet("api/v1/hotels/{hotelId:guid}/refunds")]
    public async Task<ActionResult<IReadOnlyList<RefundDto>>> GetByHotel(Guid hotelId, [FromQuery] RefundStatus? status, CancellationToken ct)
        => Ok(await service.GetByHotelAsync(hotelId, status, ct));

    [HttpGet("api/v1/refunds/{id:guid}")]
    public async Task<ActionResult<RefundDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost("api/v1/refunds/{id:guid}/approve")]
    public async Task<ActionResult<RefundDto>> Approve(Guid id, CancellationToken ct)
        => Ok(await service.ApproveAndSendToGatewayAsync(id, CurrentUserId, ct));

    [HttpPost("api/v1/refunds/{id:guid}/mark-failed")]
    public async Task<ActionResult<RefundDto>> MarkFailed(Guid id, [FromBody] MarkRefundFailedRequest request, CancellationToken ct)
        => Ok(await service.MarkFailedAsync(id, request.Reason, ct));
}

public record MarkRefundFailedRequest(string Reason);
