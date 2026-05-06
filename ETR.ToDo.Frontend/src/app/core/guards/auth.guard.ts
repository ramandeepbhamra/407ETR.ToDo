import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { AuthDialogService } from '../services/auth-dialog.service';

/**
 * Route guard that protects authenticated routes.
 *
 * If the user is not authenticated, redirects to `/` (dashboard) and
 * immediately opens the login dialog — there is no dedicated `/auth/login`
 * route; login is dialog-only by design.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const authDialog = inject(AuthDialogService);

  if (auth.isAuthenticated()) {
    return true;
  }

  authDialog.openLogin();
  return router.createUrlTree(['/']);
};
