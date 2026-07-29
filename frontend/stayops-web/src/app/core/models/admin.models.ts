import { MealPlanType } from './reservation.models';

export type RatePlanScope = 'Public' | 'Package' | 'Corporate' | 'TravelAgent';

export interface RatePlan {
  id: string;
  hotelId: string;
  code: string;
  name: string;
  mealPlan: MealPlanType;
  scope: RatePlanScope;
  cancellationPolicyId?: string;
  isActive: boolean;
}

export interface RatePlanPrice {
  id: string;
  ratePlanId: string;
  roomTypeId: string;
  occupancy: number;
  dayOfWeek?: number;
  effectiveFrom: string;
  effectiveTo: string;
  rate: number;
}

export interface Company {
  id: string;
  name: string;
  gstin: string;
  stateCode: string;
  billingAddress: string;
  creditLimit: number;
  isActive: boolean;
}

export interface CorporateRateContract {
  id: string;
  companyId: string;
  companyName: string;
  hotelId: string;
  ratePlanId: string;
  ratePlanName: string;
  contractStart: string;
  contractEnd: string;
  discountPercent?: number;
  billToCompanyByDefault: boolean;
  isActive: boolean;
}

export interface TravelAgent {
  id: string;
  name: string;
  gstin: string;
  stateCode: string;
  commissionPercent: number;
  isActive: boolean;
}

export interface AgentRateContract {
  id: string;
  travelAgentId: string;
  travelAgentName: string;
  hotelId: string;
  ratePlanId: string;
  ratePlanName: string;
  contractStart: string;
  contractEnd: string;
  discountPercent?: number;
  isActive: boolean;
}

export type PenaltyType = 'NoPenalty' | 'OneNightPenalty' | 'PercentageOfStay' | 'FullStayPenalty';

export interface CancellationPolicyRule {
  id: string;
  hoursBeforeCheckInMin?: number;
  hoursBeforeCheckInMax?: number;
  penaltyType: PenaltyType;
  penaltyValue?: number;
  appliesToNoShow: boolean;
  sortOrder: number;
  description: string;
}

export interface CancellationPolicy {
  id: string;
  hotelId: string;
  name: string;
  isActive: boolean;
  rules: CancellationPolicyRule[];
}

export type GstChargeCategory = 'RoomTariff' | 'FoodAndBeverage' | 'OtherServices';

export interface GstRule {
  id: string;
  hotelId?: string;
  chargeCategory: GstChargeCategory;
  hsnSac: string;
  minAmount?: number;
  maxAmount?: number;
  cgstRate: number;
  sgstRate: number;
  igstRate: number;
  effectiveFrom: string;
  effectiveTo?: string;
  isActive: boolean;
}

export interface NightAuditRun {
  id: string;
  hotelId: string;
  businessDate: string;
  status: 'Running' | 'Completed' | 'Failed';
  startedAtUtc: string;
  completedAtUtc?: string;
  totalRoomRevenuePosted: number;
  totalTaxPosted: number;
  staysProcessed: number;
  noShowCount: number;
  exceptionCount: number;
}

export interface NightAuditException {
  id: string;
  reservationId?: string;
  exceptionType: string;
  message: string;
  createdAtUtc: string;
}
