import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { API_BASE_URL, PUBLIC_ENDPOINTS } from '../config/api.config';
import { AuthService } from '../services/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const isApiRequest = req.url.startsWith(API_BASE_URL);
  const isPublic = PUBLIC_ENDPOINTS.some((endpoint) => req.url.startsWith(endpoint));
  const token = auth.getToken();

  // Only attach the token to our own API, never to third-party URLs or public endpoints
  const request =
    isApiRequest && !isPublic && token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      // A 401 from login means "wrong password", not "session expired"
      if (error instanceof HttpErrorResponse && error.status === 401 && isApiRequest && !isPublic) {
        auth.logout({ returnUrl: router.url });
      }
      return throwError(() => error);
    }),
  );
};
