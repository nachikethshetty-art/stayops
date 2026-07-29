namespace StayOps.Application.Guests;

public record GuestDto(
    Guid Id, string FirstName, string LastName, string Email, string Phone,
    string IdProofType, string IdProofNumber, string AddressLine1, string City, string StateCode, string Pincode, string? Gstin);

public record CreateGuestRequest(
    string FirstName, string LastName, string Email, string Phone,
    string IdProofType, string IdProofNumber, string AddressLine1, string City, string StateCode, string Pincode, string? Gstin);

public record UpdateGuestRequest(
    string FirstName, string LastName, string Email, string Phone,
    string IdProofType, string IdProofNumber, string AddressLine1, string City, string StateCode, string Pincode, string? Gstin);
