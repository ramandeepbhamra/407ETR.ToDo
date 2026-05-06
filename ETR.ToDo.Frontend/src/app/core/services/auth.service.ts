import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, filter, map, switchMap, take, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { AuthUser } from '../../features/auth/models/auth-user.model';
import { AuthLoginRequest } from '../../features/auth/models/auth-login-request.model';
import { AuthRegisterRequest } from '../../features/auth/models/auth-register-request.model';
import { AuthLoginResponse } from '../../features/auth/models/auth-login-response.model';
import { AuthRefreshTokenRequest } from '../../features/auth/models/auth-refresh-token-request.model';
import { StorageService } from './storage.service';
import { UserRole } from '../enums/user-role.enum';

/**
 * Singleton service for authentication state and token lifecycle.
 *
 * Holds the current user as a readonly signal. All HTTP requests that receive a
 * 401 are routed through `handleUnauthorized`, which queues concurrent requests
 * behind a single refresh call to avoid multiple simultaneous refresh attempts.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(StorageService);
  private readonly router = inject(Router);

  private readonly currentUserSignal = signal<AuthUser | null>(null);
  /** Readonly signal reflecting the currently authenticated user, or `null` when logged out. */
  readonly currentUser = this.currentUserSignal.asReadonly();
  /** Computed signal — `true` when a user is present in state. */
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  /** Guards a single in-flight token refresh; concurrent 401s wait on `refreshSubject`. */
  private isRefreshing = false;
  private readonly refreshSubject = new BehaviorSubject<string | null>(null);

  constructor() {
    const stored = this.storage.getUser();
    if (stored) this.currentUserSignal.set(stored);
  }

  /**
   * Authenticates with email and password.
   * On success, persists tokens and user to storage and updates `currentUser`.
   */
  login(request: AuthLoginRequest): Observable<AuthLoginResponse> {
    return this.http
      .post<ApiResponse<AuthLoginResponse>>(`${environment.apiBaseUrl}/api/auth/login`, request)
      .pipe(
        map(res => res.data!),
        tap(data => this.handleAuthSuccess(data))
      );
  }

  /**
   * Registers a new account.
   * On success, behaves identically to `login` — tokens and user are persisted.
   */
  register(request: AuthRegisterRequest): Observable<AuthLoginResponse> {
    return this.http
      .post<ApiResponse<AuthLoginResponse>>(`${environment.apiBaseUrl}/api/auth/register`, request)
      .pipe(
        map(res => res.data!),
        tap(data => this.handleAuthSuccess(data))
      );
  }

  /**
   * Updates the in-memory and stored user snapshot after a profile edit.
   * Does not re-authenticate — caller is responsible for providing valid data.
   */
  updateCurrentUser(user: AuthUser): void {
    this.storage.setUser(user);
    this.currentUserSignal.set(user);
  }

  /**
   * Revokes the stored refresh token on the server (best-effort), clears all
   * local storage, resets `currentUser` to `null`, and redirects to `/`.
   */
  logout(): void {
    const refreshToken = this.storage.getRefreshToken();
    if (refreshToken) {
      this.http
        .post(`${environment.apiBaseUrl}/api/auth/revoke`, { refreshToken } satisfies AuthRefreshTokenRequest)
        .subscribe({ error: () => {} });
    }
    this.storage.clear();
    this.currentUserSignal.set(null);
    this.router.navigate(['/']);
  }

  /** Returns the current access token from storage, or `null` if not authenticated. */
  getToken(): string | null {
    return this.storage.getToken();
  }

  /**
   * Called by `authInterceptor` when a non-auth request receives a 401.
   *
   * If no refresh is in progress, attempts to exchange the stored refresh token
   * for a new access token, then retries the original request. Subsequent 401s
   * that arrive while a refresh is in flight are queued on `refreshSubject` and
   * replayed once the new token is available.
   *
   * Calls `logout()` and throws if the refresh token is missing or the refresh
   * request itself fails.
   */
  handleUnauthorized(
    req: HttpRequest<unknown>,
    next: HttpHandlerFn
  ): Observable<HttpEvent<unknown>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshSubject.next(null);

      const refreshToken = this.storage.getRefreshToken();
      if (!refreshToken) {
        this.isRefreshing = false;
        this.logout();
        return throwError(() => new Error('Session expired'));
      }

      return this.http
        .post<ApiResponse<AuthLoginResponse>>(
          `${environment.apiBaseUrl}/api/auth/refresh`,
          { refreshToken } satisfies AuthRefreshTokenRequest
        )
        .pipe(
          map(res => res.data!),
          tap(data => {
            this.isRefreshing = false;
            this.handleAuthSuccess(data);
            this.refreshSubject.next(data.token.accessToken);
          }),
          switchMap(data =>
            next(req.clone({ headers: req.headers.set('Authorization', `Bearer ${data.token.accessToken}`) }))
          ),
          catchError(err => {
            this.isRefreshing = false;
            this.logout();
            return throwError(() => err);
          })
        );
    }

    return this.refreshSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token =>
        next(req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) }))
      )
    );
  }

  private handleAuthSuccess(data: AuthLoginResponse): void {
    this.storage.setToken(data.token.accessToken);
    this.storage.setRefreshToken(data.token.refreshToken);
    const user: AuthUser = {
      id: data.user.id,
      email: data.user.email,
      fullName: data.user.fullName,
      role: data.user.role as UserRole,
    };
    this.storage.setUser(user);
    this.currentUserSignal.set(user);
  }
}
