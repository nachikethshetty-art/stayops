import { Injectable, computed, signal } from '@angular/core';
import { Hotel } from '../models/hotel.models';

const SELECTED_HOTEL_KEY = 'stayops.selectedHotelId';

/** Tracks which hotel the user is currently operating against, persisted across reloads. */
@Injectable({ providedIn: 'root' })
export class HotelContextService {
  private readonly hotelsSignal = signal<Hotel[]>([]);
  private readonly selectedHotelIdSignal = signal<string | null>(localStorage.getItem(SELECTED_HOTEL_KEY));

  readonly hotels = this.hotelsSignal.asReadonly();
  readonly selectedHotelId = this.selectedHotelIdSignal.asReadonly();
  readonly selectedHotel = computed(() => this.hotelsSignal().find((h) => h.id === this.selectedHotelIdSignal()) ?? null);

  setHotels(hotels: Hotel[]): void {
    this.hotelsSignal.set(hotels);
    const current = this.selectedHotelIdSignal();
    if (!current || !hotels.some((h) => h.id === current)) {
      this.selectHotel(hotels[0]?.id ?? null);
    }
  }

  selectHotel(hotelId: string | null): void {
    this.selectedHotelIdSignal.set(hotelId);
    if (hotelId) {
      localStorage.setItem(SELECTED_HOTEL_KEY, hotelId);
    } else {
      localStorage.removeItem(SELECTED_HOTEL_KEY);
    }
  }

  clear(): void {
    this.hotelsSignal.set([]);
    this.selectedHotelIdSignal.set(null);
    localStorage.removeItem(SELECTED_HOTEL_KEY);
  }
}
