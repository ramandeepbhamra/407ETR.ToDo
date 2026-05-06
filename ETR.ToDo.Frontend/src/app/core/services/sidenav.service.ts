import { effect, inject, Injectable, signal } from '@angular/core';
import { ResponsiveService } from './responsive.service';

@Injectable({ providedIn: 'root' })
export class SidenavService {
  private readonly responsiveService = inject(ResponsiveService);

  readonly isOpen = signal(false);

  constructor() {
    effect(() => {
      this.isOpen.set(!this.responsiveService.smallWidth());
    });
  }

  toggle(): void {
    this.isOpen.update(v => !v);
  }
}
