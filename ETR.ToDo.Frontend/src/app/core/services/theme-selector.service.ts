import { Injectable, computed, signal, inject } from '@angular/core';
import { ResponsiveService } from './responsive.service';

@Injectable({
  providedIn: 'root',
})
export class ThemeSelectorService {
  private responsiveService = inject(ResponsiveService);

  isOpen = signal(false);

  toggle() {
    this.isOpen.set(!this.isOpen());
  }

  open() {
    this.isOpen.set(true);
  }

  close() {
    this.isOpen.set(false);
  }
}
