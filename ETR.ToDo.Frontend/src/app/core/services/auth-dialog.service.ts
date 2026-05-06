import { inject, Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

/**
 * Service for opening login and register dialogs.
 * Provides lazy-loaded authentication dialogs with automatic switching.
 * 
 * Calling openLogin() or openRegister() while another auth dialog is open
 * closes it first — enabling seamless switching between the two.
 * 
 * @example
 * ```typescript
 * constructor(private authDialogService: AuthDialogService) {}
 * 
 * showLogin() {
 *   this.authDialogService.openLogin();
 * }
 * ```
 */
@Injectable({ providedIn: 'root' })
export class AuthDialogService {
  private readonly dialog = inject(MatDialog);

  /**
   * Opens the login dialog, closing any existing auth dialog first.
   * Dialog is lazy-loaded via dynamic import.
   */
  openLogin(): void {
    this.dialog.closeAll();
    import('../../features/auth/auth-login-dialog/auth-login-dialog.component')
      .then(m => this.dialog.open(m.AuthLoginDialogComponent, { width: '440px', disableClose: false }));
  }

  /**
   * Opens the register dialog, closing any existing auth dialog first.
   * Dialog is lazy-loaded via dynamic import.
   */
  openRegister(): void {
    this.dialog.closeAll();
    import('../../features/auth/auth-register-dialog/auth-register-dialog.component')
      .then(m => this.dialog.open(m.AuthRegisterDialogComponent, { width: '520px', disableClose: false }));
  }
}
