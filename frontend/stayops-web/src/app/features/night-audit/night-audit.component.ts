import { CommonModule } from '@angular/common';
import { Component, computed, effect, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NightAuditException, NightAuditRun } from '../../core/models/admin.models';
import { HotelContextService } from '../../core/services/hotel-context.service';
import { NightAuditService } from '../../core/services/night-audit.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-night-audit',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatTableModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './night-audit.component.html',
  styleUrl: './night-audit.component.scss'
})
export class NightAuditComponent {
  readonly loading = signal(false);
  readonly running = signal(false);
  readonly history = signal<NightAuditRun[]>([]);
  readonly exceptionsByRun = signal<Record<string, NightAuditException[]>>({});
  readonly historyColumns = ['businessDate', 'status', 'staysProcessed', 'noShowCount', 'exceptionCount', 'totalRoomRevenuePosted', 'totalTaxPosted', 'actions'];

  private static readonly MIN_INTERVAL_MS = 5 * 60 * 60 * 1000;

  /**
   * Mirrors the server-side guards in NightAuditService so the button doesn't invite a click
   * that's certain to fail: (1) the hotel's business date must have actually started locally,
   * (2) at least 5 hours must have passed since the last completed run. The API is still the
   * source of truth - this is a UX hint, not a substitute for the server-side check.
   */
  readonly blockReason = computed<string | null>(() => {
    const hotel = this.hotelContext.selectedHotel();
    if (!hotel) return null;

    const todayLocal = new Intl.DateTimeFormat('en-CA', { timeZone: hotel.timeZoneId }).format(new Date());
    if (hotel.businessDate > todayLocal) {
      return `Business date ${hotel.businessDate} hasn't started yet in the hotel's local time (today is ${todayLocal}).`;
    }

    const lastCompletedAtUtc = this.history()
      .filter((r) => r.status === 'Completed' && r.completedAtUtc)
      .map((r) => new Date(r.completedAtUtc!).getTime())
      .reduce((max, t) => Math.max(max, t), 0);

    if (lastCompletedAtUtc > 0) {
      const retryAtMs = lastCompletedAtUtc + NightAuditComponent.MIN_INTERVAL_MS;
      if (retryAtMs > Date.now()) {
        return `Night audit last completed at ${new Date(lastCompletedAtUtc).toLocaleString()}. Available again after ${new Date(retryAtMs).toLocaleString()}.`;
      }
    }

    return null;
  });

  private hotelId: string | null = null;

  constructor(
    private readonly hotelContext: HotelContextService,
    private readonly nightAuditService: NightAuditService,
    private readonly notifications: NotificationService
  ) {
    effect(() => {
      const hotelId = this.hotelContext.selectedHotelId();
      if (hotelId) {
        this.hotelId = hotelId;
        this.loadHistory(hotelId);
      }
    });
  }

  private loadHistory(hotelId: string): void {
    this.loading.set(true);
    this.nightAuditService.getHistory(hotelId).subscribe({
      next: (history) => {
        this.history.set(history);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  runNightAudit(): void {
    if (!this.hotelId) return;
    this.running.set(true);
    this.nightAuditService.run(this.hotelId).subscribe({
      next: (run) => {
        this.running.set(false);
        this.notifications.success(`Night audit ${run.status}: ${run.staysProcessed} stays processed, ${run.noShowCount} no-shows, ${run.exceptionCount} exceptions.`);
        this.loadHistory(this.hotelId!);
      },
      error: () => this.running.set(false)
    });
  }

  showExceptions(run: NightAuditRun): void {
    if (!this.hotelId) return;
    this.nightAuditService.getExceptions(this.hotelId, run.id).subscribe((exceptions) => {
      this.exceptionsByRun.update((current) => ({ ...current, [run.id]: exceptions }));
    });
  }
}
