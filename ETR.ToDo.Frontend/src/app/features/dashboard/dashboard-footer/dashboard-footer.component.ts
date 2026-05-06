import { Component, inject } from '@angular/core';
import { ThemeSelectorService } from '../../../core/services/theme-selector.service';

@Component({
  selector: 'app-dashboard-footer',
  templateUrl: './dashboard-footer.component.html',
  styleUrl: './dashboard-footer.component.scss',
})
export class DashboardFooterComponent {
  readonly themeSelectorService = inject(ThemeSelectorService);
}
