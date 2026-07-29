import { CommonModule } from '@angular/common';
import { Component, effect, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { Refund } from '../../core/models/reservation.models';
import { HotelContextService } from '../../core/services/hotel-context.service';
import { RefundService } from '../../core/services/folio.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-refunds',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule],
  templateUrl: './refunds.component.html',
  styleUrl: './refunds.component.scss'
})
export class RefundsComponent {
  readonly loading = signal(false);
  readonly refunds = signal<Refund[]>([]);
  readonly columns = ['requestedAtUtc', 'amount', 'status', 'gatewayReference', 'actions'];

  private hotelId: string | null = null;

  constructor(
    private readonly hotelContext: HotelContextService,
    private readonly refundService: RefundService,
    private readonly notifications: NotificationService
  ) {
    effect(() => {
      const hotelId = this.hotelContext.selectedHotelId();
      if (hotelId) {
        this.hotelId = hotelId;
        this.load(hotelId);
      }
    });
  }

  private load(hotelId: string): void {
    this.loading.set(true);
    this.refundService.getByHotel(hotelId).subscribe({
      next: (refunds) => {
        this.refunds.set(refunds);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  approve(refund: Refund): void {
    this.refundService.approve(refund.id).subscribe(() => {
      this.notifications.success('Refund approved and sent to the mock gateway.');
      if (this.hotelId) this.load(this.hotelId);
    });
  }
}
