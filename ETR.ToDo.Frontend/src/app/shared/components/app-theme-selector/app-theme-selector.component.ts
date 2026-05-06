import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { ThemingService } from '../../../core/services/theming.service';

@Component({
  selector: 'app-theme-selector',
  templateUrl: './app-theme-selector.component.html',
  styleUrl: './app-theme-selector.component.scss',
  imports: [MatButtonModule, MatFormFieldModule, MatInput, FormsModule],
})
export class AppThemeSelectorComponent {
  readonly theming = inject(ThemingService);
}
