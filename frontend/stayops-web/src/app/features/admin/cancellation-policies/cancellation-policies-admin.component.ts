import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { CancellationPolicy } from '../../../core/models/admin.models';
import { CancellationPolicyService } from '../../../core/services/admin.service';
import { HotelContextService } from '../../../core/services/hotel-context.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-cancellation-policies-admin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatCheckboxModule],
  templateUrl: './cancellation-policies-admin.component.html',
  styleUrl: './cancellation-policies-admin.component.scss'
})
export class CancellationPoliciesAdminComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly hotelContext = inject(HotelContextService);
  private readonly policyService = inject(CancellationPolicyService);
  private readonly notifications = inject(NotificationService);

  readonly policies = signal<CancellationPolicy[]>([]);
  readonly ruleColumns = ['hoursBeforeCheckInMin', 'hoursBeforeCheckInMax', 'penaltyType', 'penaltyValue', 'appliesToNoShow', 'description', 'actions'];

  readonly policyForm = this.fb.group({ name: ['', Validators.required] });

  readonly ruleForm = this.fb.group({
    policyId: ['', Validators.required],
    hoursBeforeCheckInMin: [null as number | null],
    hoursBeforeCheckInMax: [null as number | null],
    penaltyType: ['NoPenalty', Validators.required],
    penaltyValue: [null as number | null],
    appliesToNoShow: [false],
    sortOrder: [1, Validators.required],
    description: ['', Validators.required]
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
    this.policyService.getByHotel(hotelId).subscribe((policies) => this.policies.set(policies));
  }

  createPolicy(): void {
    if (!this.hotelId || this.policyForm.invalid) return;
    this.policyService.create(this.hotelId, this.policyForm.getRawValue().name!).subscribe(() => {
      this.notifications.success('Cancellation policy created.');
      this.load(this.hotelId!);
      this.policyForm.reset();
    });
  }

  addRule(): void {
    if (!this.hotelId || this.ruleForm.invalid) return;
    const { policyId, ...request } = this.ruleForm.getRawValue();
    this.policyService.addRule(this.hotelId, policyId!, request).subscribe(() => {
      this.notifications.success('Rule added.');
      this.load(this.hotelId!);
      this.ruleForm.reset({ penaltyType: 'NoPenalty', appliesToNoShow: false, sortOrder: 1 });
    });
  }

  deleteRule(policyId: string, ruleId: string): void {
    if (!this.hotelId) return;
    this.policyService.deleteRule(this.hotelId, policyId, ruleId).subscribe(() => {
      this.notifications.success('Rule removed.');
      this.load(this.hotelId!);
    });
  }
}
