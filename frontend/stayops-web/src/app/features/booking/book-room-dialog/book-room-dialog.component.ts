import { CommonModule } from '@angular/common';
import { Component, Inject, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { AgentRateContract, CorporateRateContract } from '../../../core/models/admin.models';
import { Guest } from '../../../core/models/hotel.models';
import { RoomTypeAvailability } from '../../../core/models/reservation.models';
import { CorporateService } from '../../../core/services/admin.service';
import { GuestService } from '../../../core/services/guest.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ReservationService } from '../../../core/services/reservation.service';

export type BillingType = 'guest' | 'corporate' | 'agent';

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
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatDividerModule,
    MatRadioModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './book-room-dialog.component.html',
  styleUrl: './book-room-dialog.component.scss'
})
export class BookRoomDialogComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly dialogRef = inject(MatDialogRef<BookRoomDialogComponent>);
  private readonly guestService = inject(GuestService);
  private readonly reservationService = inject(ReservationService);
  private readonly corporateService = inject(CorporateService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly foundGuests = signal<Guest[]>([]);
  readonly selectedGuestId = signal<string | null>(null);

  /** Only contracts active today for this hotel are offered - an expired/inactive contract can't discount a new booking (see fn_ResolveNightlyRate). */
  readonly corporateContracts = signal<CorporateRateContract[]>([]);
  readonly agentContracts = signal<AgentRateContract[]>([]);

  readonly guestSearchControl = this.fb.control('');

  readonly billingType = this.fb.control<BillingType>('guest');
  readonly corporateContractId = this.fb.control('');
  readonly agentContractId = this.fb.control('');
  readonly billRoomChargeToCompany = this.fb.control(true);

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

  constructor(@Inject(MAT_DIALOG_DATA) public data: BookRoomDialogData) {
    const today = new Date().toISOString().slice(0, 10);
    this.corporateService.getContracts(data.hotelId).subscribe((contracts) =>
      this.corporateContracts.set(contracts.filter((c) => c.isActive && c.contractStart <= today && today <= c.contractEnd))
    );
    this.corporateService.getAgentContracts(data.hotelId).subscribe((contracts) =>
      this.agentContracts.set(contracts.filter((c) => c.isActive && c.contractStart <= today && today <= c.contractEnd))
    );
  }

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

  canConfirm(): boolean {
    if (!this.selectedGuestId() && !this.showNewGuestForm()) return false;
    if (this.billingType.value === 'corporate' && !this.corporateContractId.value) return false;
    if (this.billingType.value === 'agent' && !this.agentContractId.value) return false;
    return true;
  }

  confirmBooking(): void {
    if (!this.canConfirm()) return;
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
    const billingType = this.billingType.value;
    const companyId = billingType === 'corporate'
      ? this.corporateContracts().find((c) => c.id === this.corporateContractId.value)?.companyId
      : undefined;
    const travelAgentId = billingType === 'agent'
      ? this.agentContracts().find((c) => c.id === this.agentContractId.value)?.travelAgentId
      : undefined;

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
        companyId,
        travelAgentId,
        billRoomChargeToCompany: billingType === 'corporate' && this.billRoomChargeToCompany.value
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
