import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/common.models';
import { Guest } from '../models/hotel.models';

@Injectable({ providedIn: 'root' })
export class GuestService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  search(page: number, pageSize: number, search?: string): Observable<PagedResult<Guest>> {
    const params: Record<string, string> = { page: String(page), pageSize: String(pageSize) };
    if (search) params['search'] = search;
    return this.http.get<PagedResult<Guest>>(`${this.base}/guests`, { params });
  }

  getById(id: string): Observable<Guest> {
    return this.http.get<Guest>(`${this.base}/guests/${id}`);
  }

  create(request: Partial<Guest>): Observable<Guest> {
    return this.http.post<Guest>(`${this.base}/guests`, request);
  }

  update(id: string, request: Partial<Guest>): Observable<Guest> {
    return this.http.put<Guest>(`${this.base}/guests/${id}`, request);
  }
}
