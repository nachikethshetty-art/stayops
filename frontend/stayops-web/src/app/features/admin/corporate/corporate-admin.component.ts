import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { AgentRateContract, Company, CorporateRateContract, RatePlan, TravelAgent } from '../../../core/models/admin.models';
import { CorporateService, RatePlanService } from '../../../core/services/admin.service';
import { HotelContextService } from '../../../core/services/hotel-context.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-corporate-admin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatTableModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './corporate-admin.component.html',
  styleUrl: './corporate-admin.component.scss'
})
export class CorporateAdminComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly hotelContext = inject(HotelContextService);
  private readonly corporateService = inject(CorporateService);
  private readonly ratePlanService = inject(RatePlanService);
  private readonly notifications = inject(NotificationService);

  readonly companies = signal<Company[]>([]);
  readonly contracts = signal<CorporateRateContract[]>([]);
  readonly travelAgents = signal<TravelAgent[]>([]);
  readonly agentContracts = signal<AgentRateContract[]>([]);
  readonly ratePlans = signal<RatePlan[]>([]);

  readonly companyColumns = ['name', 'gstin', 'stateCode', 'creditLimit'];
  readonly contractColumns = ['companyName', 'ratePlanName', 'contractStart', 'contractEnd', 'discountPercent'];
  readonly agentColumns = ['name', 'gstin', 'commissionPercent'];
  readonly agentContractColumns = ['travelAgentName', 'ratePlanName', 'contractStart', 'contractEnd', 'discountPercent'];

  readonly companyForm = this.fb.group({
    name: ['', Validators.required],
    gstin: ['', [Validators.required, Validators.minLength(15), Validators.maxLength(15)]],
    stateCode: ['27', Validators.required],
    billingAddress: [''],
    creditLimit: [100000, Validators.required]
  });

  readonly contractForm = this.fb.group({
    companyId: ['', Validators.required],
    ratePlanId: ['', Validators.required],
    contractStart: ['', Validators.required],
    contractEnd: ['', Validators.required],
    discountPercent: [10],
    billToCompanyByDefault: [true]
  });

  readonly agentForm = this.fb.group({
    name: ['', Validators.required],
    gstin: ['', [Validators.required, Validators.minLength(15), Validators.maxLength(15)]],
    stateCode: ['27', Validators.required],
    commissionPercent: [10, Validators.required]
  });

  readonly agentContractForm = this.fb.group({
    travelAgentId: ['', Validators.required],
    ratePlanId: ['', Validators.required],
    contractStart: ['', Validators.required],
    contractEnd: ['', Validators.required],
    discountPercent: [8]
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
    this.corporateService.searchCompanies(1, 50).subscribe((result) => this.companies.set(result.items));
    this.corporateService.getContracts(hotelId).subscribe((contracts) => this.contracts.set(contracts));
    this.corporateService.getTravelAgents().subscribe((agents) => this.travelAgents.set(agents));
    this.corporateService.getAgentContracts(hotelId).subscribe((contracts) => this.agentContracts.set(contracts));
    this.ratePlanService.getByHotel(hotelId).subscribe((plans) => this.ratePlans.set(plans));
  }

  createCompany(): void {
    if (this.companyForm.invalid) return;
    this.corporateService.createCompany(this.companyForm.getRawValue()).subscribe(() => {
      this.notifications.success('Company created.');
      this.load(this.hotelId!);
      this.companyForm.reset({ stateCode: '27', creditLimit: 100000 });
    });
  }

  createContract(): void {
    if (!this.hotelId || this.contractForm.invalid) return;
    this.corporateService.createContract(this.hotelId, this.contractForm.getRawValue() as any).subscribe(() => {
      this.notifications.success('Corporate contract created.');
      this.load(this.hotelId!);
      this.contractForm.reset({ discountPercent: 10, billToCompanyByDefault: true });
    });
  }

  createAgent(): void {
    if (this.agentForm.invalid) return;
    this.corporateService.createTravelAgent(this.agentForm.getRawValue()).subscribe(() => {
      this.notifications.success('Travel agent created.');
      this.load(this.hotelId!);
      this.agentForm.reset({ stateCode: '27', commissionPercent: 10 });
    });
  }

  createAgentContract(): void {
    if (!this.hotelId || this.agentContractForm.invalid) return;
    this.corporateService.createAgentContract(this.hotelId, this.agentContractForm.getRawValue() as any).subscribe(() => {
      this.notifications.success('Agent contract created.');
      this.load(this.hotelId!);
      this.agentContractForm.reset({ discountPercent: 8 });
    });
  }
}
