import { Component, inject, computed, effect } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterOutlet } from '@angular/router';
import { CreditsComponent } from './credits.component';
import { MainMenuComponent } from './main-menu.component';
import { ResponsiveService } from '../../core/services/responsive.service';
import { SidenavService } from '../../core/services/sidenav.service';
import { ThemingService } from '../../core/services/theming.service';

@Component({
  selector: 'app-devtools',
  imports: [
    RouterOutlet,
    MatSidenavModule,
    MainMenuComponent,
    MatMenuModule,
    MatButtonModule,
    CreditsComponent,
  ],
  template: `
    <mat-drawer-container>
      <mat-drawer
        class="component-selector"
        [opened]="sidenavService.isOpen()"
        (openedChange)="sidenavService.isOpen.set($event)"
        [mode]="componentSelectorMode()"
      >
        <app-main-menu />
        <app-credits class="credits" />
      </mat-drawer>
      <mat-drawer-content [style.margin-left]="contentMargin()">
        <div class="p-6">
          <router-outlet />
        </div>
      </mat-drawer-content>
    </mat-drawer-container>
  `,
  styles: `
    mat-drawer-container {
      height: calc(100vh - 64px);
    }

    .component-selector {
      width: 200px;
      height: inherit;
      background: var(--background);
      --mat-sidenav-container-divider-color: var(--primary-light);
    }

    .credits {
      position: absolute;
      bottom: 20px;
    }
  `,
})
export class DevtoolsComponent {
  readonly themingService = inject(ThemingService);
  readonly responsiveService = inject(ResponsiveService);
  readonly sidenavService = inject(SidenavService);

  readonly componentSelectorMode = computed(() =>
    this.responsiveService.smallWidth() ? 'over' : 'side'
  );

  readonly contentMargin = computed(() =>
    this.responsiveService.smallWidth() ? '0' : '200px'
  );

  setTheme = effect(() => {
    document.body.style.setProperty(`--primary`, this.themingService.primary());
    document.body.style.setProperty(`--primary-light`, this.themingService.primaryLight());
    document.body.style.setProperty(`--ripple`, this.themingService.ripple());
    document.body.style.setProperty(`--primary-dark`, this.themingService.primaryDark());
    document.body.style.setProperty(`--background`, this.themingService.background());
    document.body.style.setProperty(`--error`, this.themingService.error());
  });
}
