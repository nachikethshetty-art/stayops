export interface HotelGroup {
  id: string;
  name: string;
  isActive: boolean;
  hotelCount: number;
}

export interface Hotel {
  id: string;
  hotelGroupId: string;
  code: string;
  name: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  pincode: string;
  stateCode: string;
  stateName: string;
  gstin: string;
  timeZoneId: string;
  businessDate: string;
  isActive: boolean;
}

export type RoomStatus = 'Available' | 'Reserved' | 'Occupied' | 'Dirty' | 'OutOfService' | 'OutOfOrder';

export interface RoomType {
  id: string;
  hotelId: string;
  code: string;
  name: string;
  description: string;
  baseOccupancy: number;
  maxOccupancy: number;
  maxChildren: number;
  isActive: boolean;
  roomCount: number;
}

export interface Room {
  id: string;
  hotelId: string;
  roomTypeId: string;
  roomTypeName: string;
  roomNumber: string;
  floor: string;
  status: RoomStatus;
  isActive: boolean;
}

export interface RoomOutOfServicePeriod {
  id: string;
  roomId: string;
  roomNumber: string;
  type: 'OutOfOrder' | 'OutOfService';
  startDate: string;
  endDate: string;
  reason: string;
  status: 'PendingApproval' | 'Approved' | 'ReturnedToService' | 'Rejected';
  approvedAtUtc?: string;
  returnedToServiceAtUtc?: string;
}

export interface Guest {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  idProofType: string;
  idProofNumber: string;
  addressLine1: string;
  city: string;
  stateCode: string;
  pincode: string;
  gstin?: string;
}

export type HousekeepingTaskType = 'CleanAfterCheckout' | 'Inspection' | 'Maintenance' | 'DeepClean';
export type HousekeepingTaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled';

export interface HousekeepingTask {
  id: string;
  hotelId: string;
  roomId: string;
  roomNumber: string;
  taskType: HousekeepingTaskType;
  status: HousekeepingTaskStatus;
  assignedToUserId?: string;
  notes: string;
  createdAtUtc: string;
  completedAtUtc?: string;
}
