import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CancellationResult,
  InventoryHold,
  Reservation,
  ReservationListItem,
  ReservationNightRate,
  RoomTypeAvailability,
  StayFolioSummary
} from '../models/reservation.models';

export interface CreateHoldRequest {
  hotelId: string;
  roomTypeId: string;
  ratePlanId: string;
  checkInDate: string;
  checkOutDate: string;
  roomsRequested: number;
  adults: number;
  children: number;
  source: 'OnlineDirect' | 'Reception';
  guestId?: string;
  companyId?: string;
  travelAgentId?: string;
  idempotencyKey: string;
}

export interface CreateReceptionReservationRequest {
  hotelId: string;
  roomTypeId: string;
  ratePlanId: string;
  checkInDate: string;
  checkOutDate: string;
  guestId: string;
  roomsRequested: number;
  adults: number;
  children: number;
  idempotencyKey: string;
  companyId?: string;
  travelAgentId?: string;
  billRoomChargeToCompany: boolean;
}

@Injectable({ providedIn: 'root' })
export class AvailabilityService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  search(
    hotelId: string,
    checkInDate: string,
    checkOutDate: string,
    adults: number,
    children: number,
    ratePlanId?: string,
    companyId?: string,
    travelAgentId?: string
  ): Observable<RoomTypeAvailability[]> {
    const params: Record<string, string> = { checkInDate, checkOutDate, adults: String(adults), children: String(children) };
    if (ratePlanId) params['ratePlanId'] = ratePlanId;
    if (companyId) params['companyId'] = companyId;
    if (travelAgentId) params['travelAgentId'] = travelAgentId;
    return this.http.get<RoomTypeAvailability[]>(`${this.base}/hotels/${hotelId}/availability`, { params });
  }
}

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  createOnlineHold(request: CreateHoldRequest): Observable<InventoryHold> {
    return this.http.post<InventoryHold>(`${this.base}/online/holds`, request);
  }

  confirmOnlinePayment(request: { holdId: string; idempotencyKey: string; amount: number; guestId?: string; billRoomChargeToCompany: boolean }): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/online/payments/webhook`, request);
  }

  createReceptionHold(hotelId: string, request: CreateHoldRequest): Observable<InventoryHold> {
    return this.http.post<InventoryHold>(`${this.base}/hotels/${hotelId}/inventory-holds`, request);
  }

  confirmHold(hotelId: string, holdId: string, request: { idempotencyKey: string; paymentReference?: string; guestId?: string; billRoomChargeToCompany: boolean }): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/hotels/${hotelId}/inventory-holds/${holdId}/confirm`, request);
  }

  createReceptionReservation(hotelId: string, request: CreateReceptionReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/hotels/${hotelId}/reservations`, request);
  }

  getByHotel(hotelId: string, checkInDate?: string, checkOutDate?: string): Observable<ReservationListItem[]> {
    const params: Record<string, string> = {};
    if (checkInDate) params['checkInDate'] = checkInDate;
    if (checkOutDate) params['checkOutDate'] = checkOutDate;
    return this.http.get<ReservationListItem[]>(`${this.base}/hotels/${hotelId}/reservations`, { params });
  }

  getById(id: string): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.base}/reservations/${id}`);
  }

  getNightRates(id: string): Observable<ReservationNightRate[]> {
    return this.http.get<ReservationNightRate[]>(`${this.base}/reservations/${id}/night-rates`);
  }

  cancel(id: string, reason?: string): Observable<CancellationResult> {
    return this.http.post<CancellationResult>(`${this.base}/reservations/${id}/cancel`, { reason });
  }

  markNoShow(id: string, reason?: string): Observable<CancellationResult> {
    return this.http.post<CancellationResult>(`${this.base}/reservations/${id}/no-show`, { reason });
  }

  getCancellation(id: string): Observable<CancellationResult> {
    return this.http.get<CancellationResult>(`${this.base}/reservations/${id}/cancellation`);
  }

  checkIn(id: string, roomId: string): Observable<StayFolioSummary[]> {
    return this.http.post<StayFolioSummary[]>(`${this.base}/reservations/${id}/check-in`, { roomId });
  }

  checkOut(id: string, forceCheckout = false): Observable<StayFolioSummary[]> {
    return this.http.post<StayFolioSummary[]>(`${this.base}/reservations/${id}/check-out`, { forceCheckout });
  }

  moveRoom(id: string, newRoomId: string, reason?: string): Observable<void> {
    return this.http.post<void>(`${this.base}/reservations/${id}/move-room`, { newRoomId, reason });
  }
}
