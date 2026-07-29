import { CommonModule } from '@angular/common';
import { Component, effect, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { NightAuditException, NightAuditRun } from '../../core/models/admin.models';
import { HotelContextService } from '../../core/services/hotel-context.service';
import { NightAuditService } from '../../core/services/night-audit.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-night-audit',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatTableModule, MatProgressSpinnerModule],
  templateUrl: './night-audit.component.html',
  styleUrl: './night-audit.component.scss'
})
export class NightAuditComponent {
  readonly loading = signal(false);
  readonly running = signal(false);
  readonly history = signal<NightAuditRun[]>([]);
  readonly exceptionsByRun = signal<Record<string, NightAuditException[]>>({});
  readonly historyColumns = ['businessDate', 'status', 'staysProcessed', 'noShowCount', 'exceptionCount', 'totalRoomRevenuePosted', 'totalTaxPosted', 'actions'];

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
