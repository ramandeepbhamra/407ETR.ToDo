import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';
import { ThemeSelectorService } from '../../../core/services/theme-selector.service';
import { AuthService } from '../../../core/services/auth.service';
import { AuthDialogService } from '../../../core/services/auth-dialog.service';
import { ResponsiveService } from '../../../core/services/responsive.service';
import { SidenavService } from '../../../core/services/sidenav.service';
import { UserRole } from '../../../core/enums/user-role.enum';

/**
 * Top navigation bar present on every page.
 * Exposes login/register dialogs for unauthenticated users and a profile dialog
 * for authenticated users. The profile dialog is lazy-loaded via dynamic `import()`
 * to keep the initial bundle lean.
 */
@Component({
  selector: 'app-navigation',
  templateUrl: './app-navigation.component.html',
  styleUrl: './app-navigation.component.scss',
  imports: [MatToolbarModule, MatButtonModule, MatIcon, MatTooltipModule, RouterLink],
})
export class AppNavigationComponent {
  private readonly authService          = inject(AuthService);
  private readonly themeSelectorService = inject(ThemeSelectorService);
  private readonly dialog               = inject(MatDialog);
  readonly authDialogService            = inject(AuthDialogService);
  readonly responsiveService            = inject(ResponsiveService);
  readonly sidenavService               = inject(SidenavService);

  readonly currentUser     = this.authService.currentUser;
  readonly isAuthenticated = computed(() => !!this.currentUser());
  readonly isAdmin         = computed(() => this.currentUser()?.role === UserRole.Admin);
  readonly isDevUser       = computed(() => this.currentUser()?.role === UserRole.Dev);

  readonly logout    = () => this.authService.logout();
  readonly openTheme = () => this.themeSelectorService.open();

  /** Opens the user profile dialog. Lazy-loaded to avoid including it in the main chunk. */
  openProfile(): void {
    import('../../../features/profile/user-profile-dialog/user-profile-dialog.component')
      .then(m => this.dialog.open(m.UserProfileDialogComponent, { width: '500px', disableClose: false }));
  }
}
