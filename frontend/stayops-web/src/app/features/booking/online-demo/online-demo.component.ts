import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Hotel } from '../../../core/models/hotel.models';
import { InventoryHold, Reservation, RoomTypeAvailability } from '../../../core/models/reservation.models';
import { GuestService } from '../../../core/services/guest.service';
import { HotelService } from '../../../core/services/hotel.service';
import { AvailabilityService, ReservationService } from '../../../core/services/reservation.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}
function addDaysIso(days: number): string {
  const d = new Date();
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

/**
 * Guest-facing online booking simulation - deliberately does not require login, mirroring a real
 * hotel website. Demonstrates the full hold -> guest details -> mock payment -> confirm flow.
 */
@Component({
  selector: 'app-online-demo',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './online-demo.component.html',
  styleUrl: './online-demo.component.scss'
})
export class OnlineDemoComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly hotelService = inject(HotelService);
  private readonly availabilityService = inject(AvailabilityService);
  private readonly reservationService = inject(ReservationService);
  private readonly guestService = inject(GuestService);

  readonly hotels = signal<Hotel[]>([]);
  readonly loading = signal(false);
  readonly results = signal<RoomTypeAvailability[]>([]);
  readonly selectedRoomType = signal<RoomTypeAvailability | null>(null);
  readonly hold = signal<InventoryHold | null>(null);
  readonly reservation = signal<Reservation | null>(null);
  readonly paying = signal(false);

  readonly searchForm = this.fb.group({
    hotelId: ['', Validators.required],
    checkInDate: [todayIso(), Validators.required],
    checkOutDate: [addDaysIso(2), Validators.required],
    adults: [2, Validators.required],
    children: [0, Validators.required]
  });

  readonly guestForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    idProofType: ['Passport', Validators.required],
    idProofNumber: ['', Validators.required],
    city: ['', Validators.required],
    stateCode: ['27', Validators.required]
  });

  constructor() {
    this.hotelService.getAccessibleHotels().subscribe((hotels) => this.hotels.set(hotels));
  }

  search(): void {
    if (this.searchForm.invalid) return;
    const { hotelId, checkInDate, checkOutDate, adults, children } = this.searchForm.getRawValue();
    this.loading.set(true);
    this.availabilityService.search(hotelId!, checkInDate!, checkOutDate!, adults!, children!).subscribe({
      next: (results) => {
        this.results.set(results);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  selectRoomType(roomType: RoomTypeAvailability): void {
    this.selectedRoomType.set(roomType);
  }

  createGuestAndHold(): void {
    if (this.guestForm.invalid) {
      this.guestForm.markAllAsTouched();
      return;
    }
    const roomType = this.selectedRoomType();
    const { hotelId, checkInDate, checkOutDate, adults, children } = this.searchForm.getRawValue();
    if (!roomType) return;

    this.loading.set(true);
    this.guestService.create(this.guestForm.getRawValue()).subscribe({
      next: (guest) => {
        this.reservationService
          .createOnlineHold({
            hotelId: hotelId!,
            roomTypeId: roomType.roomTypeId,
            ratePlanId: roomType.ratePlanId,
            checkInDate: checkInDate!,
            checkOutDate: checkOutDate!,
            roomsRequested: 1,
            adults: adults!,
            children: children!,
            source: 'OnlineDirect',
            guestId: guest.id,
            idempotencyKey: crypto.randomUUID()
          })
          .subscribe({
            next: (hold) => {
              this.hold.set(hold);
              this.loading.set(false);
            },
            error: () => this.loading.set(false)
          });
      },
      error: () => this.loading.set(false)
    });
  }

  payNow(): void {
    const hold = this.hold();
    const roomType = this.selectedRoomType();
    if (!hold || !roomType) return;

    this.paying.set(true);
    this.reservationService
      .confirmOnlinePayment({
        holdId: hold.holdId,
        idempotencyKey: crypto.randomUUID(),
        amount: roomType.totalRoomRateExclGst,
        billRoomChargeToCompany: false
      })
      .subscribe({
        next: (reservation) => {
          this.reservation.set(reservation);
          this.paying.set(false);
        },
        error: () => this.paying.set(false)
      });
  }

  startOver(): void {
    this.results.set([]);
    this.selectedRoomType.set(null);
    this.hold.set(null);
    this.reservation.set(null);
    this.guestForm.reset({ idProofType: 'Passport', stateCode: '27' });
  }
}
