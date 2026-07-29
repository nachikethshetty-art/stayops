using Microsoft.AspNetCore.Mvc.Filters;
using StayOps.Application.Common.Interfaces;
using StayOps.Application.Common.Exceptions;

namespace StayOps.Api.Filters;

/// <summary>
/// Global convenience guard: whenever an action route contains a {hotelId} segment, verify the
/// current user may act against that hotel before the action runs. This covers the common
/// "/api/v1/hotels/{hotelId}/..." shape; endpoints where the hotel is implied by a nested resource
/// (e.g. a reservationId) perform the equivalent check explicitly in their application service,
/// since only the service knows how to resolve hotelId from that resource.
/// </summary>
public class HotelScopeActionFilter(ICurrentUserService currentUser) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Anonymous endpoints (e.g. the public online-booking search/hold/payment routes) control
        // their own access explicitly via [AllowAnonymous] and have no per-user hotel scope to check.
        if (context.HttpContext.User.Identity?.IsAuthenticated == true
            && context.RouteData.Values.TryGetValue("hotelId", out var raw) && Guid.TryParse(raw?.ToString(), out var hotelId))
        {
            if (!await currentUser.CanAccessHotelAsync(hotelId, context.HttpContext.RequestAborted))
            {
                throw new ForbiddenAccessException($"You do not have access to hotel '{hotelId}'.");
            }
        }

        await next();
    }
}
