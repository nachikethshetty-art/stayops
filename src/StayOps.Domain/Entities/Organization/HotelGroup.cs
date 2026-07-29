using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Organization;

public class HotelGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}
