import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable, Subscription } from 'rxjs';

/**
 * Data interface for SessionWarningDialogComponent.
 */
export interface SessionWarningDialogData {
  /** Total countdown duration in seconds */
  totalSeconds: number;
  /** Observable that emits the current countdown value */
  countdown$: Observable<number>;
}

/**
 * Dialog component that displays a session timeout warning with live countdown.
 * 
 * Shown when the user has been idle for the configured timeout period.
 * Displays a countdown timer and provides a "Stay Logged In" button.
 * 
 * If the user clicks "Stay Logged In", the dialog closes and idle detection resets.
 * If the countdown reaches zero, the user is automatically logged out.
 * 
 * Dialog cannot be dismissed by clicking outside or pressing Escape (disableClose: true).
 * 
 * @example
 * ```typescript
 * this.dialog.open(SessionWarningDialogComponent, {
 *   width: '380px',
 *   disableClose: true,
 *   data: {
 *     totalSeconds: 30,
 *     countdown$: this.idle.onTimeoutWarning.asObservable()
 *   }
 * });
 * ```
 */
@Component({
  selector: 'app-session-warning-dialog',
  templateUrl: './session-warning-dialog.component.html',
  styleUrl: './session-warning-dialog.component.scss',
  imports: [MatDialogModule, MatButtonModule, MatIcon, MatProgressSpinnerModule],
})
export class SessionWarningDialogComponent implements OnInit, OnDestroy {
  readonly dialogRef = inject(MatDialogRef<SessionWarningDialogComponent>);
  readonly data: SessionWarningDialogData = inject(MAT_DIALOG_DATA);

  /** Current countdown value in seconds */
  readonly countdown = signal(this.data.totalSeconds);
  private sub: Subscription | null = null;

  ngOnInit(): void {
    this.sub = this.data.countdown$.subscribe(value => this.countdown.set(value));
  }

  /**
   * Closes the dialog and resets idle detection.
   * Called when user clicks "Stay Logged In" button.
   */
  stayLoggedIn(): void {
    this.dialogRef.close(true);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
