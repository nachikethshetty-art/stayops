namespace StayOps.Domain.Enums;

public enum RoomStatus
{
    Available = 0,
    Reserved = 1,
    Occupied = 2,
    Dirty = 3,
    OutOfService = 4,   // OOS - not sellable, still counted in occupancy denominator (demo policy)
    OutOfOrder = 5      // OOO - excluded from sellable inventory AND occupancy denominator
}

public enum RoomOutOfServiceType
{
    OutOfOrder = 0,
    OutOfService = 1
}

public enum OutOfServiceStatus
{
    PendingApproval = 0,
    Approved = 1,
    ReturnedToService = 2,
    Rejected = 3
}

public enum HousekeepingTaskType
{
    CleanAfterCheckout = 0,
    Inspection = 1,
    Maintenance = 2,
    DeepClean = 3
}

public enum HousekeepingTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum MealPlanType
{
    RO = 0,  // Room Only
    CP = 1,  // Continental Plan (room + breakfast)
    MAP = 2, // Modified American Plan (room + breakfast + 1 meal)
    AP = 3   // American Plan (room + all meals)
}

public enum RatePlanScope
{
    Public = 0,
    Package = 1,
    Corporate = 2,
    TravelAgent = 3
}

public enum BookingSource
{
    OnlineDirect = 0,
    Reception = 1
}

public enum InventoryHoldStatus
{
    Active = 0,
    Confirmed = 1,
    Expired = 2,
    Released = 3
}

public enum ReservationStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    CheckedIn = 2,
    CheckedOut = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum PenaltyType
{
    NoPenalty = 0,
    OneNightPenalty = 1,
    PercentageOfStay = 2,
    FullStayPenalty = 3
}

public enum CancellationTriggerType
{
    GuestCancellation = 0,
    NoShow = 1,
    LateCancellation = 2
}

public enum RefundStatus
{
    RefundRequested = 0,
    Approved = 1,
    SentToGateway = 2,
    Succeeded = 3,
    Failed = 4
}

public enum FolioType
{
    Guest = 0,
    Company = 1,
    DirectBill = 2
}

public enum FolioStatus
{
    Open = 0,
    Closed = 1
}

public enum FolioTransactionType
{
    RoomCharge = 0,
    Incidental = 1,
    Tax = 2,
    Payment = 3,
    Refund = 4,
    Adjustment = 5,
    TransferOut = 6,
    TransferIn = 7,
    Reversal = 8
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    Upi = 2,
    BankTransfer = 3,
    OnlineGateway = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}

public enum GstChargeCategory
{
    RoomTariff = 0,
    FoodAndBeverage = 1,
    OtherServices = 2
}

public enum NightAuditRunStatus
{
    Running = 0,
    Completed = 1,
    Failed = 2
}

public enum PosChargeStatus
{
    Posted = 0
}
