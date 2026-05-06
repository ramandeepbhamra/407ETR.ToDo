import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../enums/user-role.enum';

export const devUserGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated() && auth.currentUser()?.role === UserRole.Dev) {
    return true;
  }

  return router.createUrlTree(['/']);
};
