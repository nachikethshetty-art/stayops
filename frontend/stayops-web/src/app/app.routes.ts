import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { ROLES } from './core/models/auth.models';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent) },
      {
        path: 'booking/availability',
        loadComponent: () => import('./features/booking/availability/availability.component').then((m) => m.AvailabilityComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Receptionist] }
      },
      { path: 'booking/online-demo', loadComponent: () => import('./features/booking/online-demo/online-demo.component').then((m) => m.OnlineDemoComponent) },
      {
        path: 'reservations',
        loadComponent: () => import('./features/reservations/list/reservations-list.component').then((m) => m.ReservationsListComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Receptionist, ROLES.FinanceUser] }
      },
      {
        path: 'reservations/:id',
        loadComponent: () => import('./features/reservations/detail/reservation-detail.component').then((m) => m.ReservationDetailComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Receptionist, ROLES.FinanceUser] }
      },
      {
        path: 'reservations/:id/folios',
        loadComponent: () => import('./features/folios/folio-workspace.component').then((m) => m.FolioWorkspaceComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Receptionist, ROLES.FinanceUser] }
      },
      {
        path: 'housekeeping',
        loadComponent: () => import('./features/housekeeping/housekeeping-board.component').then((m) => m.HousekeepingBoardComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.Housekeeper, ROLES.Receptionist] }
      },
      {
        path: 'night-audit',
        loadComponent: () => import('./features/night-audit/night-audit.component').then((m) => m.NightAuditComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.FinanceUser] }
      },
      {
        path: 'refunds',
        loadComponent: () => import('./features/refunds/refunds.component').then((m) => m.RefundsComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.FinanceUser] }
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then((m) => m.ReportsComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager, ROLES.FinanceUser] }
      },
      {
        path: 'admin',
        loadComponent: () => import('./features/admin/admin.component').then((m) => m.AdminComponent),
        canActivate: [roleGuard],
        data: { roles: [ROLES.SuperAdmin, ROLES.HotelManager] }
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
