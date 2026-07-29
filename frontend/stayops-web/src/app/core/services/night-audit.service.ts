import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NightAuditException, NightAuditRun } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class NightAuditService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  run(hotelId: string): Observable<NightAuditRun> {
    return this.http.post<NightAuditRun>(`${this.base}/hotels/${hotelId}/night-audit/run`, {});
  }

  getHistory(hotelId: string): Observable<NightAuditRun[]> {
    return this.http.get<NightAuditRun[]>(`${this.base}/hotels/${hotelId}/night-audit/history`);
  }

  getExceptions(hotelId: string, runId: string): Observable<NightAuditException[]> {
    return this.http.get<NightAuditException[]>(`${this.base}/hotels/${hotelId}/night-audit/runs/${runId}/exceptions`);
  }
}
