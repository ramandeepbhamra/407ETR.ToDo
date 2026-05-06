import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthDialogService } from '../../../core/services/auth-dialog.service';

@Component({
  selector: 'app-dashboard-cta-section',
  templateUrl: './dashboard-cta-section.component.html',
  styleUrl: './dashboard-cta-section.component.scss',
  imports: [MatButtonModule, MatIconModule],
})
export class DashboardCtaSectionComponent {
  readonly authDialogService = inject(AuthDialogService);
}
