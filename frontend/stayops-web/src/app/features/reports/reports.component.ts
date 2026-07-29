import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { CancellationReport, CorporateReceivableRow, DailyRevenueReportRow, OccupancyReportRow } from '../../core/models/report.models';
import { HotelContextService } from '../../core/services/hotel-context.service';
import { ReportService } from '../../core/services/report.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}
function daysAgoIso(days: number): string {
  const d = new Date();
  d.setDate(d.getDate() - days);
  return d.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly hotelContext = inject(HotelContextService);
  private readonly reportService = inject(ReportService);

  readonly loading = signal(false);
  readonly occupancy = signal<OccupancyReportRow[]>([]);
  readonly revenue = signal<DailyRevenueReportRow[]>([]);
  readonly cancellationReport = signal<CancellationReport | null>(null);
  readonly receivables = signal<CorporateReceivableRow[]>([]);

  readonly occupancyColumns = ['reportDate', 'totalActiveRooms', 'outOfOrderRooms', 'occupiedRooms', 'occupancyPercent'];
  readonly revenueColumns = ['businessDate', 'roomRevenue', 'incidentalRevenue', 'totalTaxableRevenue', 'totalGst', 'totalRevenueInclGst'];
  readonly cancellationColumns = ['reservationNumber', 'triggerType', 'cancelledAtUtc', 'penaltyAmount', 'refundDueAmount', 'refundStatus'];
  readonly receivableColumns = ['companyName', 'gstin', 'creditLimit', 'openFolioCount', 'totalOutstandingBalance'];

  readonly dateForm = this.fb.group({
    fromDate: [daysAgoIso(7)],
    toDate: [todayIso()]
  });

  private hotelId: string | null = null;

  constructor() {
    effect(() => {
      const hotelId = this.hotelContext.selectedHotelId();
      if (hotelId) {
        this.hotelId = hotelId;
        this.loadAll();
      }
    });
  }

  loadAll(): void {
    if (!this.hotelId) return;
    const { fromDate, toDate } = this.dateForm.getRawValue();
    this.loading.set(true);

    this.reportService.getOccupancy(this.hotelId, fromDate!, toDate!).subscribe((rows) => this.occupancy.set(rows));
    this.reportService.getRevenueGst(this.hotelId, fromDate!, toDate!).subscribe((rows) => this.revenue.set(rows));
    this.reportService.getRefundsAndCancellations(this.hotelId, fromDate!, toDate!).subscribe((report) => this.cancellationReport.set(report));
    this.reportService.getCorporateReceivables(this.hotelId).subscribe({
      next: (rows) => {
        this.receivables.set(rows);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
