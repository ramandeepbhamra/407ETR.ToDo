import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { AppNavigationComponent } from '../../shared/components/app-navigation/app-navigation.component';
import { AppThemeSelectorComponent } from '../../shared/components/app-theme-selector/app-theme-selector.component';
import { AppBottomNavComponent } from '../../shared/components/app-bottom-nav/app-bottom-nav.component';
import { ThemeSelectorService } from '../../core/services/theme-selector.service';
import { ResponsiveService } from '../../core/services/responsive.service';
import { IdleService } from '../../core/services/idle.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  templateUrl: './app-layout.component.html',
  styleUrl: './app-layout.component.scss',
  imports: [RouterOutlet, AppNavigationComponent, MatSidenavModule, AppThemeSelectorComponent, AppBottomNavComponent],
  host: { '[class.mobile]': 'responsiveService.smallWidth()' },
})
export class AppLayoutComponent implements OnInit, OnDestroy {
  readonly themeSelectorService = inject(ThemeSelectorService);
  readonly responsiveService = inject(ResponsiveService);
  private readonly idleService  = inject(IdleService);
  private readonly authService  = inject(AuthService);

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.idleService.start();
    }
  }

  ngOnDestroy(): void {
    this.idleService.stop();
  }
}
