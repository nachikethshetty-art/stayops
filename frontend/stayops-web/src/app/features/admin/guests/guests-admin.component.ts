import { CommonModule } from '@angular/common';
import { Component, ViewChild, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { Guest } from '../../../core/models/hotel.models';
import { GuestService } from '../../../core/services/guest.service';

/** Demonstrates genuine server-side pagination/filtering against the paged Guests API (page/pageSize/search all sent to the backend). */
@Component({
  selector: 'app-guests-admin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatTableModule, MatPaginatorModule, MatFormFieldModule, MatInputModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './guests-admin.component.html',
  styleUrl: './guests-admin.component.scss'
})
export class GuestsAdminComponent {
  readonly loading = signal(false);
  readonly guests = signal<Guest[]>([]);
  readonly totalCount = signal(0);
  readonly columns = ['firstName', 'lastName', 'phone', 'email', 'city', 'stateCode'];
  readonly searchControl = new FormControl('');

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private page = 1;
  private pageSize = 10;

  constructor(private readonly guestService: GuestService) {
    this.searchControl.valueChanges.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => {
      this.page = 1;
      this.fetch();
    });
    this.fetch();
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.fetch();
  }

  private fetch(): void {
    this.loading.set(true);
    this.guestService.search(this.page, this.pageSize, this.searchControl.value ?? undefined).subscribe({
      next: (result) => {
        this.guests.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
