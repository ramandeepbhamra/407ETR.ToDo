import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { AppConfig } from '../models/app-config.model';

/**
 * Service for loading and managing application configuration from the backend.
 *
 * Configuration is loaded from `GET /api/config` during app initialization
 * and stored as a signal for reactive access throughout the application.
 *
 * Current configuration includes session timeout settings:
 * - idleTimeoutMinutes: Duration before showing warning dialog
 * - warningCountdownSeconds: Countdown duration before automatic logout
 *
 * @example
 * ```typescript
 * constructor(private configService: ConfigService) {}
 *
 * ngOnInit() {
 *   const timeout = this.configService.config().idleTimeoutMinutes;
 * }
 * ```
 */
@Injectable({ providedIn: 'root' })
export class ConfigService {
  private readonly http = inject(HttpClient);

  /**
   * Application configuration signal with default values.
   * Updated when load() is called during app initialization.
   */
  readonly config = signal<AppConfig>({
    applicationName: '407 ETR Todo App',
    applicationVersion: '1.0.0',
    supportEmail: '407ETR@yopmail.com',
    basicUserTaskLimit: 10,
    basicUserTaskListLimit: 10,
    idleTimeoutMinutes: 15,
    warningCountdownSeconds: 30
  });

  /**
   * Loads configuration from the backend API.
   * Called by app initializer before Angular bootstrap.
   *
   * @returns Observable of AppConfig that updates the config signal when complete.
   */
  load(): Observable<AppConfig> {
    return this.http
      .get<ApiResponse<AppConfig>>(`${environment.apiBaseUrl}/api/config`)
      .pipe(
        map(r => r.data!),
        tap(c => this.config.set(c))
      );
  }
}
