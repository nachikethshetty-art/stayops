import { CommonModule } from '@angular/common';
import { Component, effect, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HousekeepingTask, HousekeepingTaskStatus } from '../../core/models/hotel.models';
import { HotelContextService } from '../../core/services/hotel-context.service';
import { HotelService } from '../../core/services/hotel.service';
import { NotificationService } from '../../core/services/notification.service';

const STATUSES: HousekeepingTaskStatus[] = ['Pending', 'InProgress', 'Completed'];

@Component({
  selector: 'app-housekeeping-board',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule],
  templateUrl: './housekeeping-board.component.html',
  styleUrl: './housekeeping-board.component.scss'
})
export class HousekeepingBoardComponent {
  readonly loading = signal(false);
  readonly tasks = signal<HousekeepingTask[]>([]);
  readonly statuses = STATUSES;

  constructor(
    private readonly hotelContext: HotelContextService,
    private readonly hotelService: HotelService,
    private readonly notifications: NotificationService
  ) {
    effect(() => {
      const hotelId = this.hotelContext.selectedHotelId();
      if (hotelId) this.load(hotelId);
    });
  }

  private load(hotelId: string): void {
    this.loading.set(true);
    this.hotelService.getHousekeepingTasks(hotelId).subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  tasksByStatus(status: HousekeepingTaskStatus): HousekeepingTask[] {
    return this.tasks().filter((t) => t.status === status);
  }

  advance(task: HousekeepingTask): void {
    const hotelId = this.hotelContext.selectedHotelId();
    if (!hotelId) return;
    const nextStatus = task.status === 'Pending' ? 'InProgress' : 'Completed';
    this.hotelService.updateHousekeepingTaskStatus(hotelId, task.id, nextStatus).subscribe(() => {
      this.notifications.success(`Task moved to ${nextStatus}.`);
      this.load(hotelId);
    });
  }
}
