using StayOps.Domain.Common;

namespace StayOps.Domain.Entities.Guests;

public class Guest : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string IdProofType { get; set; } = string.Empty;   // e.g. Aadhaar, Passport, DrivingLicence
    public string IdProofNumber { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    /// <summary>Optional - set when the guest travels on their own GSTIN (rare, but supported for invoice addressing).</summary>
    public string? Gstin { get; set; }
}
