import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Hotel,
  HotelGroup,
  HousekeepingTask,
  Room,
  RoomOutOfServicePeriod,
  RoomStatus,
  RoomType
} from '../models/hotel.models';

@Injectable({ providedIn: 'root' })
export class HotelService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getAccessibleHotels(): Observable<Hotel[]> {
    return this.http.get<Hotel[]>(`${this.base}/hotels`);
  }

  getHotel(hotelId: string): Observable<Hotel> {
    return this.http.get<Hotel>(`${this.base}/hotels/${hotelId}`);
  }

  updateHotel(hotelId: string, request: Partial<Hotel>): Observable<Hotel> {
    return this.http.put<Hotel>(`${this.base}/hotels/${hotelId}`, request);
  }

  getHotelGroups(): Observable<HotelGroup[]> {
    return this.http.get<HotelGroup[]>(`${this.base}/hotel-groups`);
  }

  getRoomTypes(hotelId: string): Observable<RoomType[]> {
    return this.http.get<RoomType[]>(`${this.base}/hotels/${hotelId}/room-types`);
  }

  createRoomType(hotelId: string, request: Partial<RoomType>): Observable<RoomType> {
    return this.http.post<RoomType>(`${this.base}/hotels/${hotelId}/room-types`, request);
  }

  updateRoomType(hotelId: string, id: string, request: Partial<RoomType>): Observable<RoomType> {
    return this.http.put<RoomType>(`${this.base}/hotels/${hotelId}/room-types/${id}`, request);
  }

  getRooms(hotelId: string, status?: RoomStatus, roomTypeId?: string): Observable<Room[]> {
    const params: Record<string, string> = {};
    if (status) params['status'] = status;
    if (roomTypeId) params['roomTypeId'] = roomTypeId;
    return this.http.get<Room[]>(`${this.base}/hotels/${hotelId}/rooms`, { params });
  }

  getRoom(hotelId: string, roomId: string): Observable<Room> {
    return this.http.get<Room>(`${this.base}/hotels/${hotelId}/rooms/${roomId}`);
  }

  createRoom(hotelId: string, request: { roomTypeId: string; roomNumber: string; floor: string }): Observable<Room> {
    return this.http.post<Room>(`${this.base}/hotels/${hotelId}/rooms`, request);
  }

  changeRoomStatus(hotelId: string, roomId: string, newStatus: RoomStatus, reason?: string): Observable<Room> {
    return this.http.patch<Room>(`${this.base}/hotels/${hotelId}/rooms/${roomId}/status`, { newStatus, reason });
  }

  getOutOfServicePeriods(hotelId: string, activeOnly = false): Observable<RoomOutOfServicePeriod[]> {
    return this.http.get<RoomOutOfServicePeriod[]>(`${this.base}/hotels/${hotelId}/rooms/out-of-service`, {
      params: { activeOnly: String(activeOnly) }
    });
  }

  setOutOfOrder(
    hotelId: string,
    roomId: string,
    request: { type: 'OutOfOrder' | 'OutOfService'; startDate: string; endDate: string; reason: string }
  ): Observable<RoomOutOfServicePeriod> {
    return this.http.post<RoomOutOfServicePeriod>(`${this.base}/hotels/${hotelId}/rooms/${roomId}/out-of-service`, request);
  }

  returnToService(hotelId: string, periodId: string): Observable<RoomOutOfServicePeriod> {
    return this.http.post<RoomOutOfServicePeriod>(`${this.base}/hotels/${hotelId}/rooms/out-of-service/${periodId}/return-to-service`, {});
  }

  getHousekeepingTasks(hotelId: string, status?: string): Observable<HousekeepingTask[]> {
    const params: Record<string, string> = {};
    if (status) params['status'] = status;
    return this.http.get<HousekeepingTask[]>(`${this.base}/hotels/${hotelId}/housekeeping-tasks`, { params });
  }

  createHousekeepingTask(hotelId: string, request: { roomId: string; taskType: string; notes: string; assignedToUserId?: string }): Observable<HousekeepingTask> {
    return this.http.post<HousekeepingTask>(`${this.base}/hotels/${hotelId}/housekeeping-tasks`, request);
  }

  updateHousekeepingTaskStatus(hotelId: string, taskId: string, status: string, notes?: string): Observable<HousekeepingTask> {
    return this.http.patch<HousekeepingTask>(`${this.base}/hotels/${hotelId}/housekeeping-tasks/${taskId}/status`, { status, notes });
  }
}
