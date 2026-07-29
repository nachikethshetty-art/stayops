import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute } from '@angular/router';
import { Folio, FolioTransaction, Invoice, InvoiceLine } from '../../core/models/billing.models';
import { FolioService } from '../../core/services/folio.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-folio-workspace',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './folio-workspace.component.html',
  styleUrl: './folio-workspace.component.scss'
})
export class FolioWorkspaceComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly folioService = inject(FolioService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(false);
  readonly folios = signal<Folio[]>([]);
  readonly transactionsByFolio = signal<Record<string, FolioTransaction[]>>({});
  readonly invoices = signal<Invoice[]>([]);
  readonly invoiceLines = signal<Record<string, InvoiceLine[]>>({});
  readonly transactionColumns = ['createdAtUtc', 'type', 'description', 'amount', 'gstAmount', 'totalAmount'];

  readonly chargeForm = this.fb.group({
    chargeType: ['Incidental', Validators.required],
    chargeCategory: ['FoodAndBeverage', Validators.required],
    description: ['', Validators.required],
    taxableAmount: [0, [Validators.required, Validators.min(0.01)]]
  });

  readonly paymentForm = this.fb.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    method: ['Cash', Validators.required]
  });

  private reservationId!: string;

  ngOnInit(): void {
    this.reservationId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.folioService.getByReservation(this.reservationId).subscribe({
      next: (folios) => {
        this.folios.set(folios);
        folios.forEach((folio) => this.loadTransactions(folio.id));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
    this.folioService.getInvoices(this.reservationId).subscribe((invoices) => {
      this.invoices.set(invoices);
      invoices.forEach((invoice) =>
        this.folioService.getInvoiceLines(invoice.id).subscribe((lines) => {
          this.invoiceLines.update((current) => ({ ...current, [invoice.id]: lines }));
        })
      );
    });
  }

  private loadTransactions(folioId: string): void {
    this.folioService.getTransactions(folioId).subscribe((transactions) => {
      this.transactionsByFolio.update((current) => ({ ...current, [folioId]: transactions }));
    });
  }

  postCharge(folioId: string): void {
    if (this.chargeForm.invalid) {
      this.chargeForm.markAllAsTouched();
      return;
    }
    const request = this.chargeForm.getRawValue() as {
      chargeType: 'RoomCharge' | 'Incidental';
      chargeCategory: 'RoomTariff' | 'FoodAndBeverage' | 'OtherServices';
      description: string;
      taxableAmount: number;
    };
    this.folioService.postCharge(folioId, request).subscribe(() => {
      this.notifications.success('Charge posted.');
      this.loadTransactions(folioId);
      this.load();
      this.chargeForm.reset({ chargeType: 'Incidental', chargeCategory: 'FoodAndBeverage', taxableAmount: 0 });
    });
  }

  recordPayment(folioId: string): void {
    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }
    const request = this.paymentForm.getRawValue() as { amount: number; method: 'Cash' | 'Card' | 'Upi' | 'BankTransfer' | 'OnlineGateway' };
    this.folioService.recordPayment(folioId, { ...request, idempotencyKey: crypto.randomUUID() }).subscribe(() => {
      this.notifications.success('Payment recorded.');
      this.loadTransactions(folioId);
      this.load();
      this.paymentForm.reset({ method: 'Cash', amount: 0 });
    });
  }

  transferCharge(sourceFolioId: string, transaction: FolioTransaction): void {
    const otherFolio = this.folios().find((f) => f.id !== sourceFolioId);
    if (!otherFolio) {
      this.notifications.info('No other folio to transfer this charge to.');
      return;
    }
    this.folioService.transferCharge({ sourceTransactionId: transaction.id, destinationFolioId: otherFolio.id, reason: 'Transferred via folio workspace' }).subscribe(() => {
      this.notifications.success(`Transferred to ${otherFolio.type} folio.`);
      this.load();
    });
  }

  generateInvoice(folioId: string): void {
    this.folioService.generateInvoice(folioId).subscribe({
      next: (invoice) => {
        this.notifications.success(`Invoice ${invoice.invoiceNumber} generated.`);
        this.load();
      }
    });
  }

  canTransfer(): boolean {
    return this.folios().length > 1;
  }
}
