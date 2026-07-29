import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { RatePlan, RatePlanPrice } from '../../../core/models/admin.models';
import { RoomType } from '../../../core/models/hotel.models';
import { HotelContextService } from '../../../core/services/hotel-context.service';
import { HotelService } from '../../../core/services/hotel.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RatePlanService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-rate-plans-admin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './rate-plans-admin.component.html',
  styleUrl: './rate-plans-admin.component.scss'
})
export class RatePlansAdminComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly hotelContext = inject(HotelContextService);
  private readonly hotelService = inject(HotelService);
  private readonly ratePlanService = inject(RatePlanService);
  private readonly notifications = inject(NotificationService);

  readonly ratePlans = signal<RatePlan[]>([]);
  readonly roomTypes = signal<RoomType[]>([]);
  readonly pricesByPlan = signal<Record<string, RatePlanPrice[]>>({});
  readonly ratePlanColumns = ['code', 'name', 'mealPlan', 'scope', 'isActive'];
  readonly priceColumns = ['roomTypeName', 'occupancy', 'effectiveFrom', 'effectiveTo', 'rate'];

  readonly planForm = this.fb.group({
    code: ['', Validators.required],
    name: ['', Validators.required],
    mealPlan: ['RO', Validators.required],
    scope: ['Public', Validators.required]
  });

  readonly priceForm = this.fb.group({
    ratePlanId: ['', Validators.required],
    roomTypeId: ['', Validators.required],
    occupancy: [2, Validators.required],
    effectiveFrom: ['', Validators.required],
    effectiveTo: ['', Validators.required],
    rate: [0, [Validators.required, Validators.min(1)]]
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

  roomTypeName(id: string): string {
    return this.roomTypes().find((rt) => rt.id === id)?.name ?? id;
  }

  private load(hotelId: string): void {
    this.hotelService.getRoomTypes(hotelId).subscribe((types) => this.roomTypes.set(types));
    this.ratePlanService.getByHotel(hotelId).subscribe((plans) => {
      this.ratePlans.set(plans);
      plans.forEach((plan) => this.loadPrices(hotelId, plan.id));
    });
  }

  private loadPrices(hotelId: string, ratePlanId: string): void {
    this.ratePlanService.getPrices(hotelId, ratePlanId).subscribe((prices) => {
      this.pricesByPlan.update((current) => ({ ...current, [ratePlanId]: prices }));
    });
  }

  createRatePlan(): void {
    if (!this.hotelId || this.planForm.invalid) return;
    this.ratePlanService.create(this.hotelId, this.planForm.getRawValue() as Partial<RatePlan>).subscribe(() => {
      this.notifications.success('Rate plan created.');
      this.load(this.hotelId!);
      this.planForm.reset({ mealPlan: 'RO', scope: 'Public' });
    });
  }

  addPrice(): void {
    if (!this.hotelId || this.priceForm.invalid) return;
    const { ratePlanId, ...request } = this.priceForm.getRawValue();
    this.ratePlanService.addPrice(this.hotelId, ratePlanId!, request).subscribe(() => {
      this.notifications.success('Price added.');
      this.loadPrices(this.hotelId!, ratePlanId!);
      this.priceForm.reset({ occupancy: 2, rate: 0 });
    });
  }
}
