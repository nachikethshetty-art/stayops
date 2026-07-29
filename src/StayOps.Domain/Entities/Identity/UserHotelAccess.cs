using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Identity;

/// <summary>
/// Grants a non-SuperAdmin user operational/financial access to a specific hotel. SuperAdmin
/// users bypass this check entirely; all other roles must have a matching row for the hotel
/// they are trying to act against (see HotelScopeAuthorizationHandler).
/// </summary>
public class UserHotelAccess : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid HotelId { get; set; }
}
