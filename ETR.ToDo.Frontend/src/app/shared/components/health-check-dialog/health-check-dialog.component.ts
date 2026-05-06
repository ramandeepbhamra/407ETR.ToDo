import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';

/**
 * Non-dismissable dialog component shown when the API health check fails.
 * 
 * Displayed during app initialization if the backend API is unreachable.
 * User cannot dismiss the dialog or proceed with the application.
 * Only option is to click "Refresh" to reload the page and retry the health check.
 * 
 * This dialog indicates a critical failure - the application cannot function without API connectivity.
 * 
 * @example
 * ```typescript
 * // In app initializer
 * const healthy = await firstValueFrom(http.get('/health'));
 * if (!healthy) {
 *   dialog.open(HealthCheckDialogComponent, { disableClose: true, width: '400px' });
 * }
 * ```
 */
@Component({
  selector: 'app-health-check-dialog',
  templateUrl: './health-check-dialog.component.html',
  styleUrl: './health-check-dialog.component.scss',
  imports: [MatDialogModule, MatButtonModule, MatIcon],
})
export class HealthCheckDialogComponent {
  /**
   * Reloads the entire application.
   * Called when user clicks the "Refresh" button.
   */
  refresh(): void {
    window.location.reload();
  }
}
