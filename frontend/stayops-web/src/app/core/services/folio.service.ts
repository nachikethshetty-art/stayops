import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Folio, FolioTransaction, Invoice, InvoiceLine, Payment, PostChargeRequest, RecordPaymentRequest, TransferChargeRequest } from '../models/billing.models';
import { RefundStatus, Refund } from '../models/reservation.models';

@Injectable({ providedIn: 'root' })
export class FolioService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getByReservation(reservationId: string): Observable<Folio[]> {
    return this.http.get<Folio[]>(`${this.base}/reservations/${reservationId}/folios`);
  }

  getTransactions(folioId: string): Observable<FolioTransaction[]> {
    return this.http.get<FolioTransaction[]>(`${this.base}/folios/${folioId}/transactions`);
  }

  postCharge(folioId: string, request: PostChargeRequest): Observable<FolioTransaction> {
    return this.http.post<FolioTransaction>(`${this.base}/folios/${folioId}/charges`, request);
  }

  transferCharge(request: TransferChargeRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/folios/transfers`, request);
  }

  recordPayment(folioId: string, request: RecordPaymentRequest): Observable<Payment> {
    return this.http.post<Payment>(`${this.base}/folios/${folioId}/payments`, request);
  }

  generateInvoice(folioId: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.base}/folios/${folioId}/invoices`, {});
  }

  getInvoices(reservationId: string): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.base}/reservations/${reservationId}/invoices`);
  }

  getInvoiceLines(invoiceId: string): Observable<InvoiceLine[]> {
    return this.http.get<InvoiceLine[]>(`${this.base}/invoices/${invoiceId}/lines`);
  }
}

@Injectable({ providedIn: 'root' })
export class RefundService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getByHotel(hotelId: string, status?: RefundStatus): Observable<Refund[]> {
    const params: Record<string, string> = {};
    if (status) params['status'] = status;
    return this.http.get<Refund[]>(`${this.base}/hotels/${hotelId}/refunds`, { params });
  }

  approve(id: string): Observable<Refund> {
    return this.http.post<Refund>(`${this.base}/refunds/${id}/approve`, {});
  }

  markFailed(id: string, reason: string): Observable<Refund> {
    return this.http.post<Refund>(`${this.base}/refunds/${id}/mark-failed`, { reason });
  }
}
