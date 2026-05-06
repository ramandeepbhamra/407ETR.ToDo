import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../enums/user-role.enum';

export const adminOrDevUserGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.currentUser()?.role;

  if (auth.isAuthenticated() && (role === UserRole.Admin || role === UserRole.Dev)) {
    return true;
  }

  return router.createUrlTree(['/']);
};
