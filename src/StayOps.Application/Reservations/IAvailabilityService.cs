namespace StayOps.Application.Reservations;

public interface IAvailabilityService
{
    Task<IReadOnlyList<RoomTypeAvailabilityDto>> SearchAsync(AvailabilitySearchRequest request, CancellationToken ct = default);
}
