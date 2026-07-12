import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

export const jwtInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.getAccessToken();
  const isApiUrl = req.url.startsWith(environment.apiUrl);
  const isAuthEndpoint = req.url.includes('/auth/login') || req.url.includes('/auth/register') || req.url.includes('/auth/refresh-token');

  if (token && isApiUrl && !isAuthEndpoint) {
    req = addToken(req, token);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthEndpoint) {
        return handleUnauthorizedError(req, next, authService, router);
      }
      return throwError(() => error);
    })
  );
};

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function handleUnauthorizedError(req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService, router: Router) {
  const refreshToken = authService.getRefreshToken();
  const accessToken = authService.getAccessToken();

  if (refreshToken && accessToken) {
    return authService.refreshToken({ accessToken, refreshToken }).pipe(
      switchMap((response) => {
        if (response.success && response.data) {
          authService.setSession(response.data);
          return next(addToken(req, response.data.accessToken));
        }
        authService.clearSession();
        router.navigate(['/auth/login']);
        return throwError(() => new Error('Session expired'));
      }),
      catchError((error) => {
        authService.clearSession();
        router.navigate(['/auth/login']);
        return throwError(() => error);
      })
    );
  }

  authService.clearSession();
  router.navigate(['/auth/login']);
  return throwError(() => new Error('Session expired'));
}