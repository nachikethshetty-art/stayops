namespace StayOps.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? FullName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsSuperAdmin { get; }
    bool IsInRole(string role);

    /// <summary>True if the current user is allowed to act against the given hotel (SuperAdmin always true; others need UserHotelAccess).</summary>
    Task<bool> CanAccessHotelAsync(Guid hotelId, CancellationToken ct = default);
}
