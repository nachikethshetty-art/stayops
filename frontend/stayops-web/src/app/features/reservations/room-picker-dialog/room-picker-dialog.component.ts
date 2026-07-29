import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';
import { Room } from '../../../core/models/hotel.models';

export interface RoomPickerDialogData {
  rooms: Room[];
  title?: string;
}

@Component({
  selector: 'app-room-picker-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatListModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.title ?? 'Assign a room' }}</h2>
    <mat-dialog-content>
      @if (data.rooms.length === 0) {
        <p>No available rooms of this type right now.</p>
      } @else {
        <mat-nav-list>
          @for (room of data.rooms; track room.id) {
            <a mat-list-item [mat-dialog-close]="room">
              Room {{ room.roomNumber }} &middot; Floor {{ room.floor }}
            </a>
          }
        </mat-nav-list>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="null">Cancel</button>
    </mat-dialog-actions>
  `
})
export class RoomPickerDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: RoomPickerDialogData) {}
}
