import { CancellationTriggerType, RefundStatus } from './reservation.models';

export interface OccupancyReportRow {
  reportDate: string;
  totalActiveRooms: number;
  outOfOrderRooms: number;
  occupiedRooms: number;
  occupancyPercent: number;
}

export interface DailyRevenueReportRow {
  businessDate: string;
  roomRevenue: number;
  incidentalRevenue: number;
  totalTaxableRevenue: number;
  totalGst: number;
  totalRevenueInclGst: number;
}

export interface CancellationReportRow {
  cancellationId: string;
  reservationId: string;
  reservationNumber: string;
  triggerType: CancellationTriggerType;
  cancelledAtUtc: string;
  hotelBusinessDateAtCancellation: string;
  stayGrossAmount: number;
  penaltyAmount: number;
  penaltyGstAmount: number;
  refundDueAmount: number;
  refundId?: string;
  refundStatus?: RefundStatus;
  refundAmount?: number;
  refundCompletedAtUtc?: string;
}

export interface CancellationReportSummary {
  totalCancellations: number;
  totalNoShows: number;
  totalPenaltyCollected: number;
  totalRefundDue: number;
  totalRefundsSucceeded: number;
  totalRefundsPending: number;
}

export interface CancellationReport {
  rows: CancellationReportRow[];
  summary: CancellationReportSummary;
}

export interface CorporateReceivableRow {
  companyId: string;
  companyName: string;
  gstin: string;
  creditLimit: number;
  openFolioCount: number;
  totalOutstandingBalance: number;
}
