import { Component, effect, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemingService } from './core/services/theming.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `<router-outlet />`,
})
export class AppComponent {
  private readonly themingService = inject(ThemingService);

  setTheme = effect(() => {
    document.body.style.setProperty(`--primary`, this.themingService.primary());
    document.body.style.setProperty(`--primary-light`, this.themingService.primaryLight());
    document.body.style.setProperty(`--ripple`, this.themingService.ripple());
    document.body.style.setProperty(`--primary-dark`, this.themingService.primaryDark());
    document.body.style.setProperty(`--background`, this.themingService.background());
    document.body.style.setProperty(`--error`, this.themingService.error());
  });
}

