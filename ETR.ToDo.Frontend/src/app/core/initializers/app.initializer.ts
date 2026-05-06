import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { catchError, firstValueFrom, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConfigService } from '../services/config.service';
import { HealthCheckDialogComponent } from '../../shared/components/health-check-dialog/health-check-dialog.component';

/**
 * Application initializer function that runs before Angular bootstrap.
 * 
 * Performs two critical startup checks:
 * 1. Health check - Verifies API is reachable, shows non-dismissable dialog if down
 * 2. Config load - Fetches session timeout settings from backend
 * 
 * If health check fails, the app displays a blocking dialog with only a "Refresh" button.
 * User must fix connectivity before proceeding.
 * 
 * Registered in app.config.ts as APP_INITIALIZER.
 * 
 * @returns Async function that performs startup initialization.
 * 
 * @example
 * ```typescript
 * // In app.config.ts
 * {
 *   provide: APP_INITIALIZER,
 *   useFactory: appInitializer,
 *   multi: true
 * }
 * ```
 */
export function appInitializer(): () => Promise<void> {
  return async () => {
    const http   = inject(HttpClient);
    const config = inject(ConfigService);
    const dialog = inject(MatDialog);

    // 1. Health check — show blocking dialog if API is unreachable
    const healthy = await firstValueFrom(
      http.get(`${environment.apiBaseUrl}/health`, { responseType: 'text' }).pipe(
        catchError(() => of(null))
      )
    );

    if (!healthy) {
      dialog.open(HealthCheckDialogComponent, { disableClose: true, width: '400px' });
      return;
    }

    // 2. Load remote config (session timeout settings etc.)
    await firstValueFrom(config.load().pipe(catchError(() => of(null))));
  };
}
