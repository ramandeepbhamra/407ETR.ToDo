import { ApplicationConfig, importProvidersFrom, provideAppInitializer, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideNativeDateAdapter } from '@angular/material/core';
import { provideNgxMask } from 'ngx-mask';
import { NgIdleModule } from '@ng-idle/core';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { IdleService } from './core/services/idle.service';
import { NgIdleService } from './core/services/ng-idle.service';
import { appInitializer } from './core/initializers/app.initializer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimations(),
    provideNativeDateAdapter(),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideNgxMask(),
    importProvidersFrom(NgIdleModule.forRoot()),
    // ← swap NgIdleService with any other IdleService implementation here
    { provide: IdleService, useClass: NgIdleService },
    provideAppInitializer(appInitializer()),
  ],
};
