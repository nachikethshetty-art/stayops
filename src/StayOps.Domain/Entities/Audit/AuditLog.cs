using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Audit;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid? HotelId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? DetailsJson { get; set; }
}
