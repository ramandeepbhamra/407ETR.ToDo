import { inject, Injectable, OnDestroy } from '@angular/core';
import { DEFAULT_INTERRUPTSOURCES, Idle } from '@ng-idle/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { ConfigService } from './config.service';
import { IdleService } from './idle.service';
import { SessionWarningDialogComponent } from '../../shared/components/session-warning-dialog/session-warning-dialog.component';

/**
 * Concrete implementation of IdleService using @ng-idle/core.
 * 
 * Monitors user activity and displays a warning dialog when idle timeout is reached.
 * Automatically logs out the user if they don't respond within the countdown period.
 * 
 * To replace the library, implement IdleService with a different class
 * and update the `useClass` provider in app.config.ts.
 * 
 * Configuration is loaded from the backend via ConfigService.
 * 
 * @see IdleService
 * @see ConfigService
 */
@Injectable()
export class NgIdleService extends IdleService implements OnDestroy {
  private readonly idle          = inject(Idle);
  private readonly authService   = inject(AuthService);
  private readonly configService = inject(ConfigService);
  private readonly dialog        = inject(MatDialog);

  private dialogRef: MatDialogRef<SessionWarningDialogComponent> | null = null;
  private readonly destroy$ = new Subject<void>();

  /**
   * Starts monitoring user activity for idle timeout.
   * Loads configuration from ConfigService and sets up idle detection.
   * Opens SessionWarningDialogComponent when user becomes idle.
   */
  override start(): void {
    const config = this.configService.config();

    this.idle.setIdle(config.idleTimeoutMinutes * 60);
    this.idle.setTimeout(config.warningCountdownSeconds);
    this.idle.setInterrupts(DEFAULT_INTERRUPTSOURCES);

    // User became idle — open the warning dialog
    this.idle.onIdleStart
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.dialogRef = this.dialog.open(SessionWarningDialogComponent, {
          width: '380px',
          disableClose: true,
          data: {
            totalSeconds: config.warningCountdownSeconds,
            countdown$: this.idle.onTimeoutWarning.asObservable(),
          },
        });
      });

    // User interacted before timeout — close dialog and reset
    this.idle.onIdleEnd
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.dialogRef?.close(true);
        this.dialogRef = null;
        this.idle.watch();
      });

    // Countdown timed out — logout
    this.idle.onTimeout
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.dialogRef?.close(false);
        this.dialogRef = null;
        this.authService.logout();
      });

    this.idle.watch();
  }

  /**
   * Stops monitoring user activity and closes any open warning dialogs.
   * Call this when the user logs out.
   */
  override stop(): void {
    this.idle.stop();
    this.dialogRef?.close(false);
    this.dialogRef = null;
  }

  /**
   * Cleanup on service destruction.
   */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stop();
  }
}
