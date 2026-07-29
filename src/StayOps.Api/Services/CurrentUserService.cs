using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Identity;

namespace StayOps.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, IApplicationDbContext db) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? FullName => Principal?.FindFirstValue("full_name");

    public IReadOnlyList<string> Roles => Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsSuperAdmin => IsInRole(Application.Common.Roles.SuperAdmin);

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public async Task<bool> CanAccessHotelAsync(Guid hotelId, CancellationToken ct = default)
    {
        if (IsSuperAdmin) return true;
        var userId = UserId;
        if (userId is null) return false;

        return await db.UserHotelAccesses
            .AnyAsync(a => a.UserId == userId && a.HotelId == hotelId, ct);
    }
}
