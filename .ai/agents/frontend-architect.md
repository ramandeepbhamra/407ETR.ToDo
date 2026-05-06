# Agent: Frontend Architect

## Role
You are the frontend architect for ETR.ToDo. You make authoritative decisions on Angular architecture, component design, state management, routing, theming, and responsive behaviour. You ensure all frontend code follows Angular 21 signal-based patterns and the conventions established in this codebase.

---

## Application Architecture

### Module Structure
The app uses Angular 21 standalone components with feature-based lazy loading. There are no NgModules.

```
app/
├── app.config.ts         ← Global providers: HTTP, interceptors, animations, idle
├── app.routes.ts         ← Root routes with lazy loading and guards
├── core/                 ← Singleton, app-wide (never feature-specific)
├── features/             ← Lazy-loaded feature areas
├── layout/               ← Shell components (toolbar, sidenav container)
└── shared/               ← Reusable components, models, services used by ≥2 features
```

### Core Layer — What Belongs Here
Only singletons used across the entire application:
- `services/` — `AuthService`, `ConfigService`, `StorageService`, `ResponsiveService`, `SidenavService`, `IdleService`, `ThemingService`, `ThemeSelectorService`
- `guards/` — `authGuard`, `adminGuard`, `devUserGuard`, `adminOrDevUserGuard`
- `interceptors/` — `authInterceptor` (JWT + 401 refresh)
- `initializers/` — `appInitializer` (health check + config load before bootstrap)
- `directives/` — `TrimOnBlurDirective`
- `utils/` — `date.util.ts`
- `validators/` — `noWhitespace`, `passwordMatch`
- `enums/` — `UserRole`

**Never put feature-specific code in `core/`.**

### Shared Layer — What Belongs Here
Components, models, and services reused by two or more features:
- `components/` — `ConfirmDialogComponent`, `HealthCheckDialogComponent`, `SessionWarningDialogComponent`, `UpgradePromptDialogComponent`, `AppNavigationComponent`, `AppBottomNavComponent`, `AppThemeSelectorComponent`
- `models/` — `ApiResponse<T>`
- `services/` — `UpgradeDialogService`

---

## Routing Architecture

```typescript
// app.routes.ts
export const APP_ROUTES: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    children: [
      { path: '', component: DashboardHomeComponent },
      {
        path: 'todos',
        canActivate: [authGuard],
        loadChildren: () =>
          import('./features/todos/todos.routes').then(m => m.TODOS_ROUTES),
      },
      {
        path: 'users',
        canActivate: [adminGuard],
        loadChildren: () =>
          import('./features/users/users.routes').then(m => m.USERS_ROUTES),
      },
      {
        path: 'logs',
        canActivate: [adminOrDevUserGuard],
        loadChildren: () =>
          import('./features/api-logs/api-logs.routes').then(m => m.API_LOGS_ROUTES),
      },
      {
        path: 'dev-tools',
        canActivate: [devUserGuard],
        loadChildren: () =>
          import('./features/devtools/devtools.routes').then(m => m.DEVTOOLS_ROUTES),
      },
    ],
  },
];
```

### Guard Usage
| Guard | Condition |
|---|---|
| `authGuard` | `isAuthenticated()` must be true |
| `adminGuard` | role === `UserRole.Admin` |
| `devUserGuard` | role === `UserRole.Dev` |
| `adminOrDevUserGuard` | role is Admin or Dev |

---

## State Management Strategy

### Signals for UI State
Use signals for synchronous, component-level state:
```typescript
readonly isLoading = signal(false);
readonly tasks = signal<TodoTask[]>([]);
readonly selectedListId = signal<string | null>(null);
readonly drawerOpen = signal(false);
readonly count = computed(() => this.tasks().length);
```

### Observables for HTTP
Use RxJS Observables for all HTTP operations — never convert to signals directly from HTTP:
```typescript
// ✅ Service returns Observable
getByList(listId: string): Observable<TodoTask[]> {
  return this.http.get<ApiResponse<TodoTask[]>>(`/api/tasks?listId=${listId}`)
    .pipe(map(r => r.data!));
}

// ✅ Component subscribes and updates signal
this.todoTaskService.getByList(listId).subscribe({
  next: tasks => this.tasks.set(tasks),
  error: err => this.snackBar.open(err.error?.message, 'Close', { duration: 3000 }),
});
```

### Service Signals (App-Wide State)
For state shared across components, services use signals:
```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentUser = signal<AuthUser | null>(null);
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => !!this._currentUser());
}
```

### Effects
Use `effect()` for reactive side effects that respond to signal changes:
```typescript
constructor() {
  effect(() => {
    // Auto-close sidenav when screen shrinks to mobile
    this.isOpen.set(!this.responsiveService.smallWidth());
  });
}
```

---

## App Initializer

Runs **before** Angular bootstrap — must complete before UI renders:
```typescript
// app.initializer.ts
export function appInitializer(/* deps */): () => Promise<void> {
  return async () => {
    // 1. Health check — show non-dismissable dialog if API is down
    try {
      await firstValueFrom(http.get('/health'));
    } catch {
      // open HealthCheckDialogComponent (disableClose: true)
    }

    // 2. Load remote config — populate ConfigService
    try {
      const config = await firstValueFrom(
        http.get<ApiResponse<AppConfig>>('/api/config').pipe(map(r => r.data!))
      );
      configService.setConfig(config);
    } catch { /* use defaults */ }
  };
}
```

---

## Authentication Architecture

### Dialog-Based Auth (Non-Negotiable)
- **No route-based `/auth/login` or `/auth/register`** — ever
- Dashboard (`/`) is public — unauthenticated users see it with a login overlay
- Auth dialogs are lazy-loaded:

```typescript
// AuthDialogService
openLogin(): void {
  this.dialog.closeAll();
  import('../../features/auth/auth-login-dialog/auth-login-dialog.component')
    .then(m => this.dialog.open(m.AuthLoginDialogComponent, { width: '440px' }));
}
```

### Auth Interceptor Flow
```
Request →
  Attach "Authorization: Bearer {token}" →
    API →
      401 response? →
        Call AuthService.handleUnauthorized() →
          POST /api/auth/refresh-token →
            Success? → Retry original request with new token
            Fail?    → Logout + open login dialog
```

---

## Responsive Architecture

### ResponsiveService
Single source of truth for screen size — **never use CSS `@media` queries**:
```typescript
@Injectable({ providedIn: 'root' })
export class ResponsiveService {
  // Breakpoints:
  // small:  ≤600px      (mobile)
  // medium: 601–1000px  (tablet)
  // large:  >1000px     (desktop)

  smallWidth  = computed(() => ...);  // true on mobile
  mediumWidth = computed(() => ...);  // true on tablet
  largeWidth  = computed(() => ...);  // true on desktop
}
```

### SidenavService
Shared sidenav state for `TodoLayoutComponent` and `DevtoolsComponent`:
```typescript
@Injectable({ providedIn: 'root' })
export class SidenavService {
  readonly isOpen = signal(false);

  constructor() {
    effect(() => {
      // Open on tablet/desktop, close on mobile
      this.isOpen.set(!this.responsiveService.smallWidth());
    });
  }

  toggle(): void { this.isOpen.update(v => !v); }
}
```

### Mobile Navigation
- Top toolbar: hamburger (`menu` icon) shown only when `smallWidth() && isAuthenticated()`
- Bottom nav: `AppBottomNavComponent` — fixed bottom bar with role-aware links, shown only when `smallWidth() && isAuthenticated()`
- Top nav links hidden on mobile (handled via `@if (!responsiveService.smallWidth())`)

---

## Theming Architecture

### Theme Selector
- `ThemeSelectorService` manages the open/close state of the theme panel
- `ThemingService` applies CSS custom properties to `document.body`
- CSS custom properties: `--primary`, `--primary-light`, `--primary-dark`, `--background`, `--error`, `--ripple`
- Theme preference persisted in `localStorage`

### Material Theme Integration
```scss
// styles.scss
@use "@angular/material" as mat;
:root {
  @include mat.theme((
    color: mat.$azure-palette,
    typography: Roboto,
  ));
}
```

### Colour Rules
- Material `warn` palette for errors: `[color]="hasError ? 'warn' : undefined"`
- Use CSS vars for brand colours: `var(--primary)`, `var(--background)`
- Never hardcode hex values in SCSS or templates
- Tailwind utilities for spacing/layout only — not for colours controlled by the theme

---

## Session Management

### Idle Detection
- Abstract `IdleService` — concrete implementation is `NgIdleService` (wraps `@ng-idle/core`)
- Started when user authenticates, stopped on logout
- Timeout values from `ConfigService` (loaded from backend `/api/config`)
- `SessionWarningDialogComponent` shows live countdown

### Config Service
```typescript
readonly config = signal<AppConfig>({
  idleTimeoutMinutes: 15,         // default
  warningCountdownSeconds: 30,    // default
  supportEmail: '',
});
```
Never hardcode session timeout values — always read from `ConfigService`.

---

## Component Design Patterns

### Dialog Components
```typescript
// Standard dialog pattern — all dialogs follow this
@Component({ ... })
export class MyFeatureDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<MyFeatureDialogComponent>);
  readonly data = inject<MyDialogData>(MAT_DIALOG_DATA);

  submit(): void {
    // validate → call service → dialogRef.close(result)
  }
}
```

Always lazy-load dialogs via dynamic import in the calling component or service.

### Error Feedback Pattern
Validation errors use shake animation + Material warn colour + snackbar:
```typescript
private triggerError(errorSignal: WritableSignal<boolean>, message: string): void {
  this.snackBar.open(message, 'Close', { duration: 3000 });
  errorSignal.set(true);
  setTimeout(() => errorSignal.set(false), 600);
}
```

```html
<mat-form-field
  [color]="titleError() ? 'warn' : undefined"
  [class.shake]="titleError()"
>
```

### Two-Way Bound Inputs
```typescript
readonly isOpen = model.required<boolean>();  // model() for two-way binding
```

---

## File Naming Conventions
```
Models:     {feature}-{entity}-{operation}.model.ts
            todo-task-create-request.model.ts → TodoTaskCreateRequest

Requests:   {feature}-{entity}-{operation}-request.model.ts
            auth-login-request.model.ts → AuthLoginRequest

Forms:      {feature}-{entity}.form.ts
            auth-login.form.ts → AuthLoginForm (interface only)

Services:   {feature}-{entity}.service.ts
            todo-task.service.ts → TodoTaskService

Components: {feature}-{entity}-{action}.component.ts
            todo-task-list.component.ts → TodoTaskListComponent

Dialogs:    {feature}-{entity}-dialog.component.ts
            todo-task-list-dialog.component.ts
```

---

## Forbidden Patterns

| Forbidden | Reason |
|---|---|
| `@media` in SCSS | Use `ResponsiveService` |
| Hardcoded colours | Breaks theming |
| `NgModule` | App is fully standalone |
| `standalone: true` | Implicit since v17 |
| Route-based auth | Dialog-based only |
| `@Input()` / `@Output()` | Use `input()` / `output()` |
| Constructor injection | Use `inject()` |
| `*ngIf` / `*ngFor` | Use `@if` / `@for` |
| `errors?.required` | Use `errors?.['required']` |
| Inline template/styles | Always separate files |
| Hardcoded timeout values | Load from `ConfigService` |
| Multiple exports per file | One type per file |
