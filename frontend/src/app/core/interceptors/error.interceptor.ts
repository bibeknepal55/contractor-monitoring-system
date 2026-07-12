import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred. Please try again.';

      if (error.error instanceof ErrorEvent) {
        errorMessage = `Client Error: ${error.error.message}`;
      } else {
        switch (error.status) {
          case 400:
            if (error.error?.errors && Array.isArray(error.error.errors)) {
              errorMessage = error.error.errors.join('. ');
            } else {
              errorMessage = error.error?.message || 'Invalid request. Please check your input.';
            }
            break;
          case 401:
            errorMessage = 'Session expired. Please login again.';
            break;
          case 403:
            errorMessage = 'You do not have permission to perform this action.';
            break;
          case 404:
            errorMessage = 'The requested resource was not found.';
            break;
          case 409:
            errorMessage = error.error?.message || 'A conflict occurred. Please try again.';
            break;
          case 422:
            if (error.error?.errors && Array.isArray(error.error.errors)) {
              errorMessage = error.error.errors.join('. ');
            } else {
              errorMessage = error.error?.message || 'Validation failed.';
            }
            break;
          case 500:
            errorMessage = 'Server error. Please try again later.';
            break;
          case 0:
            errorMessage = 'Unable to connect to the server. Please check your internet connection.';
            break;
          default:
            errorMessage = error.error?.message || `Error ${error.status}: An unexpected error occurred.`;
        }
      }

      if (error.status !== 401) {
        notificationService.error(errorMessage);
      }

      return throwError(() => error);
    })
  );
};