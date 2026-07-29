import { CommonModule } from '@angular/common';
import { Component, effect, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { NightAuditRun } from '../../core/models/admin.models';
import { ReservationListItem } from '../../core/models/reservation.models';
import { Refund } from '../../core/models/reservation.models';
import { RoomOutOfServicePeriod } from '../../core/models/hotel.models';
import { DailyRevenueReportRow, OccupancyReportRow } from '../../core/models/report.models';
import { HotelContextService } from '../../core/services/hotel-context.service';
import { HotelService } from '../../core/services/hotel.service';
import { NightAuditService } from '../../core/services/night-audit.service';
import { ReportService } from '../../core/services/report.service';
import { RefundService } from '../../core/services/folio.service';
import { ReservationService } from '../../core/services/reservation.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule, MatChipsModule, MatProgressSpinnerModule, MatTableModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  readonly loading = signal(false);
  readonly occupancy = signal<OccupancyReportRow | null>(null);
  readonly revenue = signal<DailyRevenueReportRow | null>(null);
  readonly arrivals = signal<ReservationListItem[]>([]);
  readonly departures = signal<ReservationListItem[]>([]);
  readonly oooCount = signal(0);
  readonly oosPeriods = signal<RoomOutOfServicePeriod[]>([]);
  readonly pendingRefunds = signal<Refund[]>([]);
  readonly lastNightAudit = signal<NightAuditRun | null>(null);

  readonly arrivalColumns = ['reservationNumber', 'guestName', 'roomTypeName', 'status'];
  readonly departureColumns = ['reservationNumber', 'guestName', 'roomTypeName', 'status'];

  constructor(
    private readonly hotelContext: HotelContextService,
    private readonly reportService: ReportService,
    private readonly reservationService: ReservationService,
    private readonly hotelService: HotelService,
    private readonly refundService: RefundService,
    private readonly nightAuditService: NightAuditService
  ) {
    effect(() => {
      const hotelId = this.hotelContext.selectedHotelId();
      if (hotelId) {
        this.loadDashboard(hotelId);
      }
    });
  }

  private loadDashboard(hotelId: string): void {
    this.loading.set(true);
    const today = todayIso();

    forkJoin({
      occupancy: this.reportService.getOccupancy(hotelId, today, today),
      revenue: this.reportService.getRevenueGst(hotelId, today, today),
      reservations: this.reservationService.getByHotel(hotelId),
      oosPeriods: this.hotelService.getOutOfServicePeriods(hotelId, true),
      refunds: this.refundService.getByHotel(hotelId),
      nightAuditHistory: this.nightAuditService.getHistory(hotelId)
    }).subscribe({
        next: ({ occupancy, revenue, reservations, oosPeriods, refunds, nightAuditHistory }) => {
          this.occupancy.set(occupancy[0] ?? null);
          this.revenue.set(revenue[0] ?? null);
          this.arrivals.set(reservations.filter((r) => r.checkInDate.slice(0, 10) === today && r.status === 'Confirmed'));
          this.departures.set(reservations.filter((r) => r.checkOutDate.slice(0, 10) === today && r.status === 'CheckedIn'));
          this.oosPeriods.set(oosPeriods);
          this.oooCount.set(oosPeriods.filter((p) => p.type === 'OutOfOrder').length);
          this.pendingRefunds.set(refunds.filter((r) => r.status === 'RefundRequested' || r.status === 'Approved' || r.status === 'SentToGateway'));
          this.lastNightAudit.set(nightAuditHistory[0] ?? null);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }
}
