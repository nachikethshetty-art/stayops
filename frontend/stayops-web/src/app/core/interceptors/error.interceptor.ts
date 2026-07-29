import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models/common.models';
import { NotificationService } from '../services/notification.service';

/** Surfaces ProblemDetails errors from the API as a snackbar so components don't each need their own boilerplate. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status !== 401) {
        const problem = error.error as ProblemDetails | undefined;
        const message = problem?.detail ?? problem?.title ?? error.message ?? 'An unexpected error occurred.';
        notifications.error(message);
      }
      return throwError(() => error);
    })
  );
};
