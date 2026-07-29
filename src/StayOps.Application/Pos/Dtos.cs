using StayOps.Domain.Enums;

namespace StayOps.Application.Pos;

public record PostPosChargeRequest(Guid HotelId, string OutletCode, string PosReferenceNumber, string RoomNumber, decimal Amount, string Description, GstChargeCategory ChargeCategory);

public record PosChargeResultDto(Guid PosChargeId, Guid FolioId, Guid FolioTransactionId, decimal Amount, decimal FolioBalance, bool WasDuplicate);
