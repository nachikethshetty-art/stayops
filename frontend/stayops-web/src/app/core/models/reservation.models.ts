export type MealPlanType = 'RO' | 'CP' | 'MAP' | 'AP';
export type BookingSource = 'OnlineDirect' | 'Reception';
export type InventoryHoldStatus = 'Active' | 'Confirmed' | 'Expired' | 'Released';
export type ReservationStatus = 'PendingPayment' | 'Confirmed' | 'CheckedIn' | 'CheckedOut' | 'Cancelled' | 'NoShow';
export type RefundStatus = 'RefundRequested' | 'Approved' | 'SentToGateway' | 'Succeeded' | 'Failed';
export type CancellationTriggerType = 'GuestCancellation' | 'NoShow' | 'LateCancellation';

export interface RoomTypeAvailability {
  roomTypeId: string;
  roomTypeName: string;
  baseOccupancy: number;
  maxOccupancy: number;
  totalRoomRateExclGst: number;
  averageNightlyRate: number;
  ratePlanId: string;
  ratePlanName: string;
  mealPlan: MealPlanType;
  rateSource: string;
  availableCount: number;
}

export interface InventoryHold {
  holdId: string;
  hotelId: string;
  roomTypeId: string;
  ratePlanId: string;
  checkInDate: string;
  checkOutDate: string;
  roomsRequested: number;
  status: InventoryHoldStatus;
  source: BookingSource;
  expiresAtUtc: string;
  guestId?: string;
  companyId?: string;
  travelAgentId?: string;
  reservationId?: string;
}

export interface Reservation {
  id: string;
  hotelId: string;
  reservationNumber: string;
  guestId: string;
  companyId?: string;
  travelAgentId?: string;
  roomTypeId: string;
  ratePlanId: string;
  checkInDate: string;
  checkOutDate: string;
  roomsBooked: number;
  adults: number;
  children: number;
  status: ReservationStatus;
  source: BookingSource;
  inventoryHoldId?: string;
  businessDateCreated: string;
  billRoomChargeToCompany: boolean;
  createdAtUtc: string;
}

export interface ReservationListItem {
  reservationId: string;
  hotelId: string;
  reservationNumber: string;
  status: ReservationStatus;
  source: BookingSource;
  checkInDate: string;
  checkOutDate: string;
  roomsBooked: number;
  adults: number;
  children: number;
  guestId: string;
  guestName: string;
  guestPhone: string;
  guestEmail: string;
  roomTypeId: string;
  roomTypeName: string;
  ratePlanId: string;
  ratePlanName: string;
  companyId?: string;
  companyName?: string;
  createdAtUtc: string;
}

export interface ReservationNightRate {
  stayDate: string;
  roomRate: number;
  mealPlan: MealPlanType;
  cgstRate: number;
  sgstRate: number;
  igstRate: number;
}

export interface CancellationResult {
  id: string;
  reservationId: string;
  triggerType: CancellationTriggerType;
  cancelledAtUtc: string;
  hotelBusinessDateAtCancellation: string;
  hoursBeforeCheckIn: number;
  stayGrossAmount: number;
  penaltyAmount: number;
  penaltyGstAmount: number;
  refundDueAmount: number;
  reason: string;
  refundId?: string;
  refundStatus?: RefundStatus;
}

export interface Refund {
  id: string;
  cancellationId: string;
  reservationId: string;
  amount: number;
  status: RefundStatus;
  gatewayReference?: string;
  failureReason?: string;
  requestedAtUtc: string;
  approvedAtUtc?: string;
  sentToGatewayAtUtc?: string;
  completedAtUtc?: string;
}

export type FolioType = 'Guest' | 'Company' | 'DirectBill';
export type FolioStatus = 'Open' | 'Closed';

export interface StayFolioSummary {
  folioId: string;
  folioType: FolioType;
  folioStatus: FolioStatus;
  balance: number;
}
