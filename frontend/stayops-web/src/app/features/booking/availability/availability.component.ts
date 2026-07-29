import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { RoomTypeAvailability } from '../../../core/models/reservation.models';
import { HotelContextService } from '../../../core/services/hotel-context.service';
import { AvailabilityService } from '../../../core/services/reservation.service';
import { BookRoomDialogComponent } from '../book-room-dialog/book-room-dialog.component';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}
function addDaysIso(days: number): string {
  const d = new Date();
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-availability',
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
    MatTableModule,
    MatDialogModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './availability.component.html',
  styleUrl: './availability.component.scss'
})
export class AvailabilityComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly availabilityService = inject(AvailabilityService);
  private readonly hotelContext = inject(HotelContextService);
  private readonly dialog = inject(MatDialog);

  readonly loading = signal(false);
  readonly searched = signal(false);
  readonly results = signal<RoomTypeAvailability[]>([]);
  readonly columns = ['roomTypeName', 'mealPlan', 'ratePlanName', 'averageNightlyRate', 'totalRoomRateExclGst', 'availableCount', 'actions'];

  readonly form = this.fb.group({
    checkInDate: [todayIso(), Validators.required],
    checkOutDate: [addDaysIso(1), Validators.required],
    adults: [2, [Validators.required, Validators.min(1)]],
    children: [0, [Validators.required, Validators.min(0)]]
  });

  search(): void {
    const hotelId = this.hotelContext.selectedHotelId();
    if (!hotelId || this.form.invalid) return;

    const { checkInDate, checkOutDate, adults, children } = this.form.getRawValue();
    this.loading.set(true);
    this.searched.set(true);
    this.availabilityService.search(hotelId, checkInDate!, checkOutDate!, adults!, children!).subscribe({
      next: (results) => {
        this.results.set(results);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  book(roomType: RoomTypeAvailability): void {
    const hotelId = this.hotelContext.selectedHotelId();
    if (!hotelId) return;
    const { checkInDate, checkOutDate, adults, children } = this.form.getRawValue();

    this.dialog.open(BookRoomDialogComponent, {
      width: '520px',
      data: { hotelId, roomType, checkInDate, checkOutDate, adults, children }
    });
  }
}
