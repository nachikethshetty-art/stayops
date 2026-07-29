using StayOps.Domain.Enums;

namespace StayOps.Application.Housekeeping;

public record HousekeepingTaskDto(
    Guid Id, Guid HotelId, Guid RoomId, string RoomNumber, HousekeepingTaskType TaskType,
    HousekeepingTaskStatus Status, Guid? AssignedToUserId, string Notes, DateTime CreatedAtUtc, DateTime? CompletedAtUtc);

public record CreateHousekeepingTaskRequest(Guid RoomId, HousekeepingTaskType TaskType, string Notes, Guid? AssignedToUserId);
public record UpdateHousekeepingTaskStatusRequest(HousekeepingTaskStatus Status, Guid? AssignedToUserId, string? Notes);
