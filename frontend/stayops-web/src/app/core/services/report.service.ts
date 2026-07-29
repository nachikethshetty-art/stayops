import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CancellationReport, CorporateReceivableRow, DailyRevenueReportRow, OccupancyReportRow } from '../models/report.models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getOccupancy(hotelId: string, fromDate: string, toDate: string): Observable<OccupancyReportRow[]> {
    return this.http.get<OccupancyReportRow[]>(`${this.base}/hotels/${hotelId}/reports/occupancy`, { params: { fromDate, toDate } });
  }

  getRevenueGst(hotelId: string, fromDate: string, toDate: string): Observable<DailyRevenueReportRow[]> {
    return this.http.get<DailyRevenueReportRow[]>(`${this.base}/hotels/${hotelId}/reports/revenue-gst`, { params: { fromDate, toDate } });
  }

  getRefundsAndCancellations(hotelId: string, fromDate: string, toDate: string): Observable<CancellationReport> {
    return this.http.get<CancellationReport>(`${this.base}/hotels/${hotelId}/reports/refunds-cancellations`, { params: { fromDate, toDate } });
  }

  getCorporateReceivables(hotelId: string): Observable<CorporateReceivableRow[]> {
    return this.http.get<CorporateReceivableRow[]>(`${this.base}/hotels/${hotelId}/reports/corporate-receivables`);
  }
}
