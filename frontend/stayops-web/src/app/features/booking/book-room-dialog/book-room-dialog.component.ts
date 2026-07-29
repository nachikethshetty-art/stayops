import { CommonModule } from '@angular/common';
import { Component, Inject, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { Guest } from '../../../core/models/hotel.models';
import { RoomTypeAvailability } from '../../../core/models/reservation.models';
import { GuestService } from '../../../core/services/guest.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ReservationService } from '../../../core/services/reservation.service';

export interface BookRoomDialogData {
  hotelId: string;
  roomType: RoomTypeAvailability;
  checkInDate: string;
  checkOutDate: string;
  adults: number;
  children: number;
}

@Component({
  selector: 'app-book-room-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatCheckboxModule, MatProgressSpinnerModule],
  templateUrl: './book-room-dialog.component.html',
  styleUrl: './book-room-dialog.component.scss'
})
export class BookRoomDialogComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly dialogRef = inject(MatDialogRef<BookRoomDialogComponent>);
  private readonly guestService = inject(GuestService);
  private readonly reservationService = inject(ReservationService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly foundGuests = signal<Guest[]>([]);
  readonly selectedGuestId = signal<string | null>(null);

  readonly guestSearchControl = this.fb.control('');

  readonly newGuestForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    idProofType: ['Aadhaar', Validators.required],
    idProofNumber: ['', Validators.required],
    addressLine1: [''],
    city: [''],
    stateCode: ['27', Validators.required],
    pincode: ['']
  });

  showNewGuestForm = signal(false);

  constructor(@Inject(MAT_DIALOG_DATA) public data: BookRoomDialogData) {}

  searchGuests(): void {
    const term = this.guestSearchControl.value ?? '';
    if (!term.trim()) return;
    this.guestService.search(1, 10, term).subscribe((result) => this.foundGuests.set(result.items));
  }

  selectGuest(guest: Guest): void {
    this.selectedGuestId.set(guest.id);
    this.showNewGuestForm.set(false);
  }

  toggleNewGuestForm(): void {
    this.showNewGuestForm.set(!this.showNewGuestForm());
    this.selectedGuestId.set(null);
  }

  confirmBooking(): void {
    if (this.showNewGuestForm()) {
      if (this.newGuestForm.invalid) {
        this.newGuestForm.markAllAsTouched();
        return;
      }
      this.loading.set(true);
      this.guestService.create(this.newGuestForm.getRawValue()).subscribe({
        next: (guest) => this.createReservation(guest.id),
        error: () => this.loading.set(false)
      });
    } else if (this.selectedGuestId()) {
      this.createReservation(this.selectedGuestId()!);
    }
  }

  private createReservation(guestId: string): void {
    this.loading.set(true);
    this.reservationService
      .createReceptionReservation(this.data.hotelId, {
        hotelId: this.data.hotelId,
        roomTypeId: this.data.roomType.roomTypeId,
        ratePlanId: this.data.roomType.ratePlanId,
        checkInDate: this.data.checkInDate,
        checkOutDate: this.data.checkOutDate,
        guestId,
        roomsRequested: 1,
        adults: this.data.adults,
        children: this.data.children,
        idempotencyKey: crypto.randomUUID(),
        billRoomChargeToCompany: false
      })
      .subscribe({
        next: (reservation) => {
          this.loading.set(false);
          this.notifications.success(`Reservation ${reservation.reservationNumber} confirmed.`);
          this.dialogRef.close(reservation);
          this.router.navigate(['/reservations', reservation.id]);
        },
        error: () => this.loading.set(false)
      });
  }
}
