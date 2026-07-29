namespace StayOps.Application.Common;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string HotelManager = "HotelManager";
    public const string Receptionist = "Receptionist";
    public const string FinanceUser = "FinanceUser";
    public const string Housekeeper = "Housekeeper";
    public const string POSSystem = "POSSystem";

    public static readonly string[] All =
    [
        SuperAdmin, HotelManager, Receptionist, FinanceUser, Housekeeper, POSSystem
    ];

    /// <summary>Roles whose access to a hotel's operational/financial data must be checked against UserHotelAccess.</summary>
    public static readonly string[] HotelScopedRoles =
    [
        HotelManager, Receptionist, FinanceUser, Housekeeper, POSSystem
    ];
}
