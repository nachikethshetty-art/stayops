import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

/** Attaches the bearer token to every API request, and retries once after a silent refresh on 401. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const token = auth.getAccessToken();
  const authedReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authedReq).pipe(
    catchError((error: unknown) => {
      const isAuthEndpoint = req.url.includes('/auth/login') || req.url.includes('/auth/refresh');
      if (error instanceof HttpErrorResponse && error.status === 401 && !isAuthEndpoint && auth.getRefreshToken()) {
        return auth.refresh().pipe(
          switchMap((response) => {
            const retried = req.clone({ setHeaders: { Authorization: `Bearer ${response.accessToken}` } });
            return next(retried);
          }),
          catchError((refreshError) => {
            auth.forceLogout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
