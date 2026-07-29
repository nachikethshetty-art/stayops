import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { Room, RoomOutOfServicePeriod, RoomType } from '../../../core/models/hotel.models';
import { HotelContextService } from '../../../core/services/hotel-context.service';
import { HotelService } from '../../../core/services/hotel.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-rooms-admin',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatChipsModule
  ],
  templateUrl: './rooms-admin.component.html',
  styleUrl: './rooms-admin.component.scss'
})
export class RoomsAdminComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly hotelContext = inject(HotelContextService);
  private readonly hotelService = inject(HotelService);
  private readonly notifications = inject(NotificationService);

  readonly roomTypes = signal<RoomType[]>([]);
  readonly rooms = signal<Room[]>([]);
  readonly oosPeriods = signal<RoomOutOfServicePeriod[]>([]);
  readonly roomTypeColumns = ['code', 'name', 'baseOccupancy', 'maxOccupancy', 'roomCount'];
  readonly roomColumns = ['roomNumber', 'roomTypeName', 'floor', 'status'];
  readonly oosColumns = ['roomNumber', 'type', 'startDate', 'endDate', 'reason', 'status', 'actions'];

  readonly roomTypeForm = this.fb.group({
    code: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
    baseOccupancy: [2, Validators.required],
    maxOccupancy: [2, Validators.required],
    maxChildren: [0, Validators.required]
  });

  readonly roomForm = this.fb.group({
    roomTypeId: ['', Validators.required],
    roomNumber: ['', Validators.required],
    floor: ['', Validators.required]
  });

  readonly oosForm = this.fb.group({
    roomId: ['', Validators.required],
    type: ['OutOfOrder', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    reason: ['', Validators.required]
  });

  private hotelId: string | null = null;

  constructor() {
    effect(() => {
      const hotelId = this.hotelContext.selectedHotelId();
      if (hotelId) {
        this.hotelId = hotelId;
        this.load(hotelId);
      }
    });
  }

  private load(hotelId: string): void {
    this.hotelService.getRoomTypes(hotelId).subscribe((types) => this.roomTypes.set(types));
    this.hotelService.getRooms(hotelId).subscribe((rooms) => this.rooms.set(rooms));
    this.hotelService.getOutOfServicePeriods(hotelId).subscribe((periods) => this.oosPeriods.set(periods));
  }

  createRoomType(): void {
    if (!this.hotelId || this.roomTypeForm.invalid) return;
    this.hotelService.createRoomType(this.hotelId, this.roomTypeForm.getRawValue()).subscribe(() => {
      this.notifications.success('Room type created.');
      this.load(this.hotelId!);
      this.roomTypeForm.reset({ baseOccupancy: 2, maxOccupancy: 2, maxChildren: 0 });
    });
  }

  createRoom(): void {
    if (!this.hotelId || this.roomForm.invalid) return;
    const request = this.roomForm.getRawValue() as { roomTypeId: string; roomNumber: string; floor: string };
    this.hotelService.createRoom(this.hotelId, request).subscribe(() => {
      this.notifications.success('Room created.');
      this.load(this.hotelId!);
      this.roomForm.reset();
    });
  }

  setOutOfOrder(): void {
    if (!this.hotelId || this.oosForm.invalid) return;
    const { roomId, type, startDate, endDate, reason } = this.oosForm.getRawValue();
    this.hotelService
      .setOutOfOrder(this.hotelId, roomId!, { type: type as 'OutOfOrder' | 'OutOfService', startDate: startDate!, endDate: endDate!, reason: reason! })
      .subscribe(() => {
        this.notifications.success('Room marked out of service.');
        this.load(this.hotelId!);
        this.oosForm.reset({ type: 'OutOfOrder' });
      });
  }

  returnToService(period: RoomOutOfServicePeriod): void {
    if (!this.hotelId) return;
    this.hotelService.returnToService(this.hotelId, period.id).subscribe(() => {
      this.notifications.success('Room returned to service.');
      this.load(this.hotelId!);
    });
  }
}
