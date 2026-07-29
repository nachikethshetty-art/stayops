import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Route data usage: { roles: ['SuperAdmin', 'HotelManager'] } */
export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const allowedRoles = (route.data?.['roles'] as string[] | undefined) ?? [];
  if (allowedRoles.length === 0 || auth.hasAnyRole(...allowedRoles)) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
