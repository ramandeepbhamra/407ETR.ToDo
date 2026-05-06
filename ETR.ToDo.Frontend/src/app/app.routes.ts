import { Routes } from '@angular/router';
import { DashboardHomeComponent } from './features/dashboard/dashboard-home/dashboard-home.component';
import { DevtoolsComponent } from './features/devtools/devtools.component';
import { ButtonsComponent } from './features/devtools/buttons.component';
import { InputsComponent } from './features/devtools/inputs.component';
import { StepperComponent } from './features/devtools/stepper.component';
import { TabsComponent } from './features/devtools/tabs.component';
import { ProgressComponent } from './features/devtools/progress.component';
import { DialogComponent } from './features/devtools/dialog.component';
import { PanelsComponent } from './features/devtools/panels.component';
import { TableComponent } from './features/devtools/table.component';
import { AppLayoutComponent } from './layout/app-layout/app-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { devUserGuard } from './core/guards/dev-user.guard';
import { adminOrDevUserGuard } from './core/guards/admin-or-dev-user.guard';

/**
 * Root route configuration.
 *
 * Auth is dialog-only — there is no `/auth/login` page route. The `auth` path
 * exists only to satisfy lazy-loading conventions; `AUTH_ROUTES` is an empty array.
 *
 * Guard hierarchy:
 * - `authGuard`         — any authenticated user; redirects to `/` + opens login dialog
 * - `adminGuard`        — Admin role only
 * - `devUserGuard`      — Dev role only
 * - `adminOrDevUserGuard` — Admin or Dev role
 *
 * Feature routes (`users`, `todos`, `logs`) are lazy-loaded via their own route files.
 */
export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },
  {
    path: '',
    component: AppLayoutComponent,
    children: [{ path: '', component: DashboardHomeComponent }],
  },
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', component: DashboardHomeComponent },
      {
        path: 'dev-tools',
        canActivate: [devUserGuard],
        component: DevtoolsComponent,
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'buttons' },
          { path: 'buttons', component: ButtonsComponent },
          { path: 'inputs', component: InputsComponent },
          { path: 'progress', component: ProgressComponent },
          { path: 'dialog', component: DialogComponent },
          { path: 'panels', component: PanelsComponent },
          { path: 'table', component: TableComponent },
          { path: 'stepper', component: StepperComponent },
          { path: 'tabs', component: TabsComponent },
        ],
      },
      {
        path: 'users',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/users/users.routes').then(m => m.USER_ROUTES),
      },
      {
        path: 'todos',
        loadChildren: () => import('./features/todos/todos.routes').then(m => m.TODO_ROUTES),
      },
      {
        path: 'logs',
        canActivate: [adminOrDevUserGuard],
        loadChildren: () => import('./features/api-logs/api-logs.routes').then(m => m.API_LOGS_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: 'auth/login' },
];
