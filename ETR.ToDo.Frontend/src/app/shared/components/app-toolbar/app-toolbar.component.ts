import { Component, computed, inject, model } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { ResponsiveService } from '../../../core/services/responsive.service';

@Component({
  selector: 'app-toolbar',
  templateUrl: './app-toolbar.component.html',
  styleUrl: './app-toolbar.component.scss',
  imports: [MatIcon, MatButtonModule],
})
export class AppToolbarComponent {
  readonly componentSelectorOpen = model.required<boolean>();
  readonly themeSelectorOpen = model.required<boolean>();
  readonly responsiveService = inject(ResponsiveService);

  readonly title = computed(() =>
    `${this.responsiveService.largeWidth() ? 'Angular Material 3 ' : ''}Theme Builder`
  );
}
