import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { ThemeSelectorService } from '../../../core/services/theme-selector.service';

@Component({
  selector: 'app-dashboard-hero-section',
  templateUrl: './dashboard-hero-section.component.html',
  styleUrl: './dashboard-hero-section.component.scss',
  imports: [MatButtonModule],
})
export class DashboardHeroSectionComponent {
  readonly themeSelectorService = inject(ThemeSelectorService);
}
