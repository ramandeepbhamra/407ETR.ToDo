import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { StorageService } from '../services/storage.service';
import { AuthService } from '../services/auth.service';

/**
 * Functional HTTP interceptor that attaches the Bearer token to every outgoing
 * request and handles 401 responses.
 *
 * On a 401 from any non-auth endpoint, delegates to `AuthService.handleUnauthorized`
 * which attempts a token refresh. Concurrent 401s are queued there so only one
 * refresh call is made. Auth endpoints (`/api/auth/`) are excluded to avoid
 * infinite loops on login/refresh failures.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const storage = inject(StorageService);
  const auth = inject(AuthService);

  const token = storage.getToken();
  const authReq = token
    ? req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) })
    : req;

  return next(authReq).pipe(
    catchError(error => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !req.url.includes('/api/auth/')
      ) {
        return auth.handleUnauthorized(req, next);
      }
      return throwError(() => error);
    })
  );
};
