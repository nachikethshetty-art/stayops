import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { Folio } from '../../../core/models/billing.models';
import { Room } from '../../../core/models/hotel.models';
import { ROLES } from '../../../core/models/auth.models';
import { CancellationResult, Reservation, ReservationNightRate } from '../../../core/models/reservation.models';
import { AuthService } from '../../../core/services/auth.service';
import { FolioService } from '../../../core/services/folio.service';
import { HotelService } from '../../../core/services/hotel.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';
import { RoomPickerDialogComponent } from '../room-picker-dialog/room-picker-dialog.component';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatDividerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './reservation-detail.component.html',
  styleUrl: './reservation-detail.component.scss'
})
export class ReservationDetailComponent implements OnInit {
  readonly loading = signal(false);
  readonly reservation = signal<Reservation | null>(null);
  readonly nightRates = signal<ReservationNightRate[]>([]);
  readonly cancellation = signal<CancellationResult | null>(null);
  readonly folios = signal<Folio[]>([]);
  readonly nightRateColumns = ['stayDate', 'roomRate', 'mealPlan', 'cgstRate', 'sgstRate', 'igstRate'];

  private reservationId!: string;

  /** Stay-mutating actions (check-in/out, cancel, no-show, move-room) are reception/management only - see ReservationsController. */
  readonly canManageStay: boolean;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly reservationService: ReservationService,
    private readonly hotelService: HotelService,
    private readonly folioService: FolioService,
    private readonly notifications: NotificationService,
    private readonly dialog: MatDialog,
    private readonly auth: AuthService
  ) {
    this.canManageStay = this.auth.hasAnyRole(ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Receptionist);
  }

  ngOnInit(): void {
    this.reservationId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.reservationService.getById(this.reservationId).subscribe({
      next: (reservation) => {
        this.reservation.set(reservation);
        this.reservationService.getNightRates(this.reservationId).subscribe((rates) => this.nightRates.set(rates));

        if (reservation.status === 'Cancelled' || reservation.status === 'NoShow') {
          this.reservationService.getCancellation(this.reservationId).subscribe((c) => this.cancellation.set(c));
        }
        if (reservation.status === 'CheckedIn' || reservation.status === 'CheckedOut') {
          this.folioService.getByReservation(this.reservationId).subscribe((folios) => this.folios.set(folios));
        }

        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  checkIn(): void {
    const reservation = this.reservation();
    if (!reservation) return;

    this.hotelService.getRooms(reservation.hotelId, 'Available', reservation.roomTypeId).subscribe((rooms) => {
      const ref = this.dialog.open(RoomPickerDialogComponent, { width: '420px', data: { rooms } });
      ref.afterClosed().subscribe((room: Room | undefined) => {
        if (!room) return;
        this.reservationService.checkIn(this.reservationId, room.id).subscribe(() => {
          this.notifications.success(`Checked in to room ${room.roomNumber}.`);
          this.load();
        });
      });
    });
  }

  checkOut(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Check out guest', message: 'Confirm checkout? The guest folio must be settled first.' }
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.reservationService.checkOut(this.reservationId).subscribe(() => {
        this.notifications.success('Guest checked out.');
        this.load();
      });
    });
  }

  moveRoom(): void {
    const reservation = this.reservation();
    if (!reservation) return;
    this.hotelService.getRooms(reservation.hotelId, 'Available', reservation.roomTypeId).subscribe((rooms) => {
      const ref = this.dialog.open(RoomPickerDialogComponent, { width: '420px', data: { rooms, title: 'Move to room' } });
      ref.afterClosed().subscribe((room: Room | undefined) => {
        if (!room) return;
        this.reservationService.moveRoom(this.reservationId, room.id, 'Guest requested move').subscribe(() => {
          this.notifications.success(`Moved to room ${room.roomNumber}.`);
          this.load();
        });
      });
    });
  }

  cancel(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Cancel reservation', message: 'Penalty will be computed from the policy snapshot and current time. Continue?' }
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.reservationService.cancel(this.reservationId, 'Cancelled via reception').subscribe((result) => {
        this.cancellation.set(result);
        this.notifications.success(`Cancelled. Penalty: Rs.${result.penaltyAmount + result.penaltyGstAmount}. Refund due: Rs.${result.refundDueAmount}.`);
        this.load();
      });
    });
  }

  markNoShow(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Mark as no-show', message: 'This will apply the no-show penalty rule. Continue?' }
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.reservationService.markNoShow(this.reservationId, 'Marked manually').subscribe((result) => {
        this.cancellation.set(result);
        this.notifications.success('Reservation marked as no-show.');
        this.load();
      });
    });
  }

  goToFolio(): void {
    this.router.navigate(['/reservations', this.reservationId, 'folios']);
  }
}
