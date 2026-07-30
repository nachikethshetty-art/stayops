import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ROLES } from '../../core/models/auth.models';
import { AuthService } from '../../core/services/auth.service';
import { HotelContextService } from '../../core/services/hotel-context.service';
import { HotelService } from '../../core/services/hotel.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatSelectModule
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly hotelContext = inject(HotelContextService);
  private readonly hotelService = inject(HotelService);

  readonly currentUser = this.auth.currentUser;
  readonly hotels = this.hotelContext.hotels;
  readonly selectedHotelId = this.hotelContext.selectedHotelId;
  readonly sidenavOpen = signal(true);

  readonly navItems = computed<NavItem[]>(() => {
    const roles = this.auth.roles();
    const items: NavItem[] = [
      { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
      { label: 'Availability & Booking', icon: 'event_available', route: '/booking/availability', roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Receptionist] },
      { label: 'Online Booking Demo', icon: 'shopping_cart', route: '/booking/online-demo' },
      { label: 'Reservations', icon: 'book_online', route: '/reservations', roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Receptionist, ROLES.FinanceUser] },
      { label: 'Housekeeping', icon: 'cleaning_services', route: '/housekeeping', roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Housekeeper, ROLES.Receptionist] },
      { label: 'Night Audit', icon: 'nightlight', route: '/night-audit', roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.FinanceUser] },
      { label: 'Reports', icon: 'bar_chart', route: '/reports', roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.FinanceUser] },
      { label: 'Refunds', icon: 'currency_rupee', route: '/refunds', roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.FinanceUser] },
      { label: 'Administration', icon: 'settings', route: '/admin', roles: [ROLES.SuperAdmin, ROLES.HotelManager] }
    ];
    return items.filter((item) => !item.roles || item.roles.some((r) => roles.includes(r)));
  });

  ngOnInit(): void {
    this.hotelService.getAccessibleHotels().subscribe((hotels) => this.hotelContext.setHotels(hotels));
  }

  onHotelChange(hotelId: string): void {
    this.hotelContext.selectHotel(hotelId);
  }

  logout(): void {
    this.auth.logout();
  }
}
