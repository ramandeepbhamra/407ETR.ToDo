import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { AuthService } from '../../../core/services/auth.service';
import { AuthDialogService } from '../../../core/services/auth-dialog.service';

@Component({
  selector: 'app-dashboard-pricing-section',
  templateUrl: './dashboard-pricing-section.component.html',
  styleUrl: './dashboard-pricing-section.component.scss',
  imports: [MatButtonModule, MatCardModule, MatListModule, MatIconModule],
})
export class DashboardPricingSectionComponent {
  private readonly authService = inject(AuthService);
  readonly authDialogService   = inject(AuthDialogService);
  readonly isAuthenticated     = computed(() => !!this.authService.currentUser());
}
