namespace StayOps.Application.Hotels;

public record HotelGroupDto(Guid Id, string Name, bool IsActive, int HotelCount);

public record CreateHotelGroupRequest(string Name);
public record UpdateHotelGroupRequest(string Name, bool IsActive);

public record HotelDto(
    Guid Id, Guid HotelGroupId, string Code, string Name,
    string AddressLine1, string AddressLine2, string City, string Pincode,
    string StateCode, string StateName, string Gstin, string TimeZoneId,
    DateOnly BusinessDate, bool IsActive);

public record CreateHotelRequest(
    Guid HotelGroupId, string Code, string Name,
    string AddressLine1, string AddressLine2, string City, string Pincode,
    string StateCode, string StateName, string Gstin, string TimeZoneId);

public record UpdateHotelRequest(
    string Name, string AddressLine1, string AddressLine2, string City, string Pincode,
    string StateCode, string StateName, string Gstin, string TimeZoneId, bool IsActive);
