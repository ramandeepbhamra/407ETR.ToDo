import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { AuthService } from '../../../core/services/auth.service';
import { ResponsiveService } from '../../../core/services/responsive.service';
import { UserRole } from '../../../core/enums/user-role.enum';

@Component({
  selector: 'app-bottom-nav',
  templateUrl: './app-bottom-nav.component.html',
  styleUrl: './app-bottom-nav.component.scss',
  imports: [RouterLink, RouterLinkActive, MatIcon, MatRippleModule],
})
export class AppBottomNavComponent {
  private readonly authService = inject(AuthService);
  readonly responsiveService = inject(ResponsiveService);

  readonly currentUser = this.authService.currentUser;
  readonly isAuthenticated = computed(() => !!this.currentUser());
  readonly isAdmin = computed(() => this.currentUser()?.role === UserRole.Admin);
  readonly isDevUser = computed(() => this.currentUser()?.role === UserRole.Dev);
  readonly isAdminOrDev = computed(() => this.isAdmin() || this.isDevUser());
}
