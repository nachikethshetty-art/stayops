import { FolioStatus, FolioType } from './reservation.models';

export type FolioTransactionType = 'RoomCharge' | 'Incidental' | 'Tax' | 'Payment' | 'Refund' | 'Adjustment' | 'TransferOut' | 'TransferIn' | 'Reversal';
export type PaymentMethod = 'Cash' | 'Card' | 'Upi' | 'BankTransfer' | 'OnlineGateway';
export type PaymentStatus = 'Pending' | 'Succeeded' | 'Failed';
export type GstChargeCategory = 'RoomTariff' | 'FoodAndBeverage' | 'OtherServices';

export interface Folio {
  id: string;
  reservationId: string;
  type: FolioType;
  ownerCompanyId?: string;
  status: FolioStatus;
  balance: number;
  openedAtUtc: string;
  closedAtUtc?: string;
}

export interface FolioTransaction {
  id: string;
  folioId: string;
  type: FolioTransactionType;
  description: string;
  amount: number;
  gstAmount: number;
  totalAmount: number;
  reversalOfTransactionId?: string;
  businessDate: string;
  postedByUserId?: string;
  sourceReference?: string;
  createdAtUtc: string;
}

export interface PostChargeRequest {
  chargeType: 'RoomCharge' | 'Incidental';
  chargeCategory: GstChargeCategory;
  description: string;
  taxableAmount: number;
}

export interface TransferChargeRequest {
  sourceTransactionId: string;
  destinationFolioId: string;
  reason: string;
}

export interface RecordPaymentRequest {
  amount: number;
  method: PaymentMethod;
  gatewayReference?: string;
  idempotencyKey?: string;
}

export interface Payment {
  id: string;
  reservationId: string;
  folioId?: string;
  amount: number;
  method: PaymentMethod;
  status: PaymentStatus;
  gatewayReference?: string;
  createdAtUtc: string;
}

export interface Invoice {
  id: string;
  reservationId: string;
  folioId: string;
  invoiceNumber: string;
  invoiceDate: string;
  supplierGstin: string;
  supplierStateCode: string;
  billedPartyName?: string;
  billedPartyGstin?: string;
  billedPartyStateCode: string;
  placeOfSupplyStateCode: string;
  isInterState: boolean;
  totalTaxableValue: number;
  totalCgst: number;
  totalSgst: number;
  totalIgst: number;
  totalAmount: number;
}

export interface InvoiceLine {
  id: string;
  invoiceId: string;
  description: string;
  hsnSac: string;
  taxableValue: number;
  cgstRate: number;
  cgstAmount: number;
  sgstRate: number;
  sgstAmount: number;
  igstRate: number;
  igstAmount: number;
  lineTotal: number;
}
