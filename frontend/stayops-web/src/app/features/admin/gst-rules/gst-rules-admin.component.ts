import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { GstRule } from '../../../core/models/admin.models';
import { AuthService } from '../../../core/services/auth.service';
import { GstRuleService } from '../../../core/services/admin.service';
import { HotelContextService } from '../../../core/services/hotel-context.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-gst-rules-admin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatTableModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatCheckboxModule, MatChipsModule],
  templateUrl: './gst-rules-admin.component.html',
  styleUrl: './gst-rules-admin.component.scss'
})
export class GstRulesAdminComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly hotelContext = inject(HotelContextService);
  private readonly gstRuleService = inject(GstRuleService);
  private readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);

  readonly rules = signal<GstRule[]>([]);
  readonly columns = ['scope', 'chargeCategory', 'hsnSac', 'minAmount', 'maxAmount', 'cgstRate', 'sgstRate', 'igstRate', 'effectiveFrom'];

  readonly isSuperAdmin = this.auth.hasAnyRole('SuperAdmin');

  readonly form = this.fb.group({
    chargeCategory: ['RoomTariff', Validators.required],
    hsnSac: ['996311', Validators.required],
    minAmount: [0],
    maxAmount: [null as number | null],
    cgstRate: [6, Validators.required],
    sgstRate: [6, Validators.required],
    igstRate: [12, Validators.required],
    effectiveFrom: ['', Validators.required],
    effectiveTo: [null as string | null],
    hotelSpecific: [true]
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
    this.gstRuleService.getForHotel(hotelId).subscribe((rules) => this.rules.set(rules));
  }

  createRule(): void {
    if (!this.hotelId || this.form.invalid) return;
    this.gstRuleService.create(this.hotelId, this.form.getRawValue()).subscribe(() => {
      this.notifications.success('GST rule created.');
      this.load(this.hotelId!);
      this.form.reset({ chargeCategory: 'RoomTariff', minAmount: 0, cgstRate: 6, sgstRate: 6, igstRate: 12, hotelSpecific: true });
    });
  }
}
