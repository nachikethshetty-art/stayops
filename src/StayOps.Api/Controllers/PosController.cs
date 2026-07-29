using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Application.Common;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Pos;

namespace StayOps.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.POSSystem},{Roles.SuperAdmin}")]
public class PosController(IPosChargeService service, ICurrentUserService currentUser) : ControllerBase
{
    public const string ApiKeyHeaderName = "X-Pos-Api-Key";

    /// <summary>
    /// POS outlet posts a charge to whichever guest is currently checked into the given room.
    /// Two layers of authentication: a JWT bearer token for the POSSystem/outlet integration user,
    /// plus the outlet-specific API key in the X-Pos-Api-Key header (validated against the outlet
    /// row belonging to the hotel in the request body).
    /// </summary>
    [HttpPost("api/v1/pos/post-charge")]
    public async Task<ActionResult<PosChargeResultDto>> PostCharge([FromBody] PostPosChargeRequest request, CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ForbiddenAccessException($"Missing required '{ApiKeyHeaderName}' header.");
        }

        if (!await currentUser.CanAccessHotelAsync(request.HotelId, ct))
        {
            throw new ForbiddenAccessException($"You do not have access to hotel '{request.HotelId}'.");
        }

        var result = await service.PostChargeAsync(apiKey.ToString(), request, ct);
        return Ok(result);
    }
}
