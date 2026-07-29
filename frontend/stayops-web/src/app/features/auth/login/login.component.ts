import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly hidePassword = signal(true);

  readonly form = this.fb.group({
    userNameOrEmail: ['', Validators.required],
    password: ['', Validators.required]
  });

  readonly demoAccounts = [
    { userName: 'superadmin', role: 'SuperAdmin' },
    { userName: 'manager.mumbai', role: 'HotelManager (Mumbai)' },
    { userName: 'reception.mumbai', role: 'Receptionist (Mumbai)' },
    { userName: 'finance.mumbai', role: 'FinanceUser (Mumbai)' },
    { userName: 'housekeeping.mumbai', role: 'Housekeeper (Mumbai)' }
  ];

  fillDemoAccount(userName: string): void {
    this.form.patchValue({ userNameOrEmail: userName, password: 'Passw0rd!123' });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const { userNameOrEmail, password } = this.form.getRawValue();
    this.auth.login({ userNameOrEmail: userNameOrEmail!, password: password! }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Invalid username/email or password.');
      }
    });
  }
}
