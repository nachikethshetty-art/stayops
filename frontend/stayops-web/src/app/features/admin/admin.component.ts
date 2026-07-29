import { Component } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { RatePlansAdminComponent } from './rate-plans/rate-plans-admin.component';
import { CorporateAdminComponent } from './corporate/corporate-admin.component';
import { CancellationPoliciesAdminComponent } from './cancellation-policies/cancellation-policies-admin.component';
import { GstRulesAdminComponent } from './gst-rules/gst-rules-admin.component';
import { RoomsAdminComponent } from './rooms/rooms-admin.component';
import { GuestsAdminComponent } from './guests/guests-admin.component';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [
    MatTabsModule,
    RatePlansAdminComponent,
    CorporateAdminComponent,
    CancellationPoliciesAdminComponent,
    GstRulesAdminComponent,
    RoomsAdminComponent,
    GuestsAdminComponent
  ],
  template: `
    <h1>Administration</h1>
    <mat-tab-group>
      <mat-tab label="Rooms &amp; Room Types"><app-rooms-admin /></mat-tab>
      <mat-tab label="Rate Plans"><app-rate-plans-admin /></mat-tab>
      <mat-tab label="Corporate &amp; Agents"><app-corporate-admin /></mat-tab>
      <mat-tab label="Cancellation Policies"><app-cancellation-policies-admin /></mat-tab>
      <mat-tab label="GST Rules"><app-gst-rules-admin /></mat-tab>
      <mat-tab label="Guests"><app-guests-admin /></mat-tab>
    </mat-tab-group>
  `
})
export class AdminComponent {}
