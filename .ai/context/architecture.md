# Architecture — ETR.ToDo

## Backend: Clean Architecture

### Layer Diagram

```
┌──────────────────────────────────────────┐
│           ETR.ToDo.Web.Host               │  Entry point, controllers, Program.cs
├──────────────────────────────────────────┤
│           ETR.ToDo.Web.Core               │  ApiControllerBase, JWT, middleware
├──────────────────────────────────────────┤
│           ETR.ToDo.Services               │  Business logic, Mapperly mappers, seeder
├──────────────────────────────────────────┤
│        ETR.ToDo.Services.Core             │  Service interfaces, all DTOs
├──────────────────────────────────────────┤
│           ETR.ToDo.Core.EF                │  DbContext, Fluent API configs, migrations
├──────────────────────────────────────────┤
│            ETR.ToDo.Core                  │  Entities, IRepository<T,K> (pure domain)
├──────────────────────────────────────────┤
│         ETR.ToDo.Core.Shared              │  Enums, constants, config POCOs
└──────────────────────────────────────────┘
```

**Dependency rule:** inner layers never reference outer layers. `ETR.ToDo.Core` has zero external NuGet dependencies.

### Entity Anatomy

Every entity extends `BaseEntity<Guid>`:
```csharp
public abstract class BaseEntity<TKey>
{
    public TKey Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
}
```
Audit fields are populated automatically by `ToDoDbContext.SaveChangesAsync()`. Soft delete is mandatory — there are no hard-delete operations on any entity.

### Domain Relationships

```
User ──< TaskList    (User.Id → TaskList.UserId)
User ──< TodoTask    (User.Id → TodoTask.UserId)
User ──< RefreshToken
TaskList ──< TodoTask  (TaskList.Id → TodoTask.ListId)
TodoTask ──< SubTask   (TodoTask.Id → SubTask.TodoTaskId)
```

### Repository Pattern

`IRepository<TEntity, TKey>` in `ETR.ToDo.Core` — generic interface providing:
- `GetAll()` → `IQueryable<T>` (EF query composition happens here)
- `GetByIdAsync(id)`
- `InsertAsync(entity)`
- `UpdateAsync(entity)`
- `DeleteAsync(entity)` — sets `IsDeleted = true` (soft delete)
- `SaveChangesAsync()`

Concrete implementation `Repository<T,K>` lives in `ETR.ToDo.Core.EF`. Services depend only on the interface.

### Service Layer

Services own all business logic. Pattern:
1. Validate input / enforce limits
2. Compose query via `_repo.GetAll()` with `.Include()`, `.Where()`, `.WhereIf()`
3. Map entity ↔ DTO via Mapperly (never manual property assignment)
4. Persist via `_repo.InsertAsync()` / `UpdateAsync()` / `DeleteAsync()` + `SaveChangesAsync()`

Exception semantics (caught by `ExceptionHandlingMiddleware`):
- `KeyNotFoundException` → 404
- `UnauthorizedAccessException` → 403
- `InvalidOperationException` → 400

### API Response Envelope

All endpoints return `ApiResponse<T>`:
```json
{ "success": true, "data": { ... }, "message": null }
{ "success": false, "data": null, "message": "Reason", "statusCode": 404 }
```

Controllers return only helper methods from `ApiControllerBase`:
`Success()`, `Created()`, `NotFound()`, `BadRequest()`, `Unauthorized()`, `Forbidden()`.

### Authentication Architecture

```
POST /api/auth/login
  → BCrypt.Verify(password, hash)
  → JwtTokenService.GenerateToken()   (60 min JWT)
  → RefreshTokenService.CreateAsync() (7-day refresh token)
  → { accessToken, refreshToken, user }

POST /api/auth/refresh-token
  → Validate refresh token (not expired, not revoked)
  → Issue new JWT + new refresh token
  → Revoke old refresh token (rotation)
  → 401 on failure → frontend logout + login dialog
```

JWT claims: `sub` (GUID), `email`, `role`, `exp`. Always set `MapInboundClaims = false`.

---

## Frontend: Angular 21 Standalone Signals

### Module Structure

```
src/app/
├── app.config.ts         ← Global providers (HTTP, interceptors, animations, idle)
├── app.routes.ts         ← Root routes, lazy loading, guards
├── core/                 ← App-wide singletons only
│   ├── services/         ← AuthService, ResponsiveService, SidenavService, ThemingService ...
│   ├── guards/           ← authGuard, adminGuard, devUserGuard, adminOrDevUserGuard
│   ├── interceptors/     ← authInterceptor (JWT attach + 401 refresh)
│   ├── initializers/     ← appInitializer (health check + config load)
│   ├── directives/       ← TrimOnBlurDirective
│   ├── enums/            ← UserRole
│   ├── utils/            ← date.util.ts
│   └── validators/       ← noWhitespace, passwordMatch
├── features/             ← Lazy-loaded feature areas
│   ├── auth/             ← Login + Register dialogs
│   ├── todos/            ← TodoLayout, TodoSidebar, TodoItem, TodoForm, dialogs
│   ├── users/            ← User management
│   ├── api-logs/         ← Log viewer
│   └── devtools/         ← Component browser
├── layout/               ← AppLayoutComponent (shell with toolbar + router-outlet)
└── shared/
    ├── components/       ← AppNavigationComponent, AppBottomNavComponent,
    │                        ConfirmDialogComponent, AppThemeSelectorComponent ...
    ├── models/           ← ApiResponse<T>
    └── services/         ← UpgradeDialogService
```

### Routing

Lazy-loaded feature routes behind guards:
```typescript
{ path: 'todos',     canActivate: [authGuard],          loadChildren: () => import('./features/todos/todos.routes') }
{ path: 'users',     canActivate: [adminGuard],          loadChildren: () => import('./features/users/users.routes') }
{ path: 'logs',      canActivate: [adminOrDevUserGuard], loadChildren: () => import('./features/api-logs/api-logs.routes') }
{ path: 'dev-tools', canActivate: [devUserGuard],        loadChildren: () => import('./features/devtools/devtools.routes') }
```

Dashboard (`/`) is public — `AppLayoutComponent` wraps all routes.

### State Management

| Concern | Mechanism |
|---|---|
| Component UI state | `signal()` — synchronous, local |
| Derived state | `computed()` — memoised |
| Reactive side effects | `effect()` |
| Shared app-wide state | Service-level `signal()` (e.g. `AuthService._currentUser`) |
| HTTP | RxJS `Observable` — subscribe in component, write to signal |
| Two-way binding | `model()` / `model.required()` |

Never convert HTTP Observables directly to signals. Subscribe and call `.set()`.

### Responsive Layout

`ResponsiveService` is the only source of screen-size truth:
```typescript
smallWidth  = computed(() => ...);  // ≤600px  — mobile
mediumWidth = computed(() => ...);  // 601–1000px — tablet
largeWidth  = computed(() => ...);  // >1000px — desktop
```

`SidenavService` (singleton in `core/services`) manages open/close state for both `TodoLayoutComponent` and `DevtoolsComponent`:
```typescript
readonly isOpen = signal(false);
constructor() {
  effect(() => { this.isOpen.set(!this.responsiveService.smallWidth()); });
}
toggle(): void { this.isOpen.update(v => !v); }
```

Mobile navigation: `AppBottomNavComponent` fixed at bottom, shown only when `smallWidth() && isAuthenticated()`. Role-aware links.

### Theming

`ThemingService` writes CSS custom properties to `document.body`:
```
--primary  --primary-light  --primary-dark  --background  --error  --ripple
```
`ThemeSelectorService` controls panel open/close state. Preference persisted in `localStorage`. Never hardcode hex values — always `var(--primary)` etc.

### Auth Flow

Auth is dialog-only. `AuthDialogService` lazy-loads `AuthLoginDialogComponent` and `AuthRegisterDialogComponent` via dynamic `import()`. No `/auth/login` route exists.

`authInterceptor`:
1. Attach `Authorization: Bearer {token}` to every request
2. On 401 → call `AuthService.handleUnauthorized()` → POST `/api/auth/refresh-token`
3. Success → retry original request with new token
4. Failure → logout + open login dialog

### App Initializer

Runs before Angular bootstrap:
1. `GET /health` — show non-dismissable `HealthCheckDialogComponent` if API is down
2. `GET /api/config` — populate `ConfigService` (idle timeouts, support email)

### Session Management

`IdleService` (abstract) wraps `@ng-idle/core`. Started on login, stopped on logout. Timeout values from `ConfigService` — never hardcoded. `SessionWarningDialogComponent` shows live countdown.

---

## Cross-Cutting: Error UX Pattern

Validation errors use three coordinated signals:
1. **Snackbar** — human-readable message (3-second auto-dismiss)
2. **Warn colour** — `[color]="hasError() ? 'warn' : undefined"` on `mat-form-field`
3. **Shake animation** — `[class.shake]="hasError()"` + `@keyframes shake` in `styles.scss`

```typescript
private triggerError(errorSignal: WritableSignal<boolean>, message: string): void {
  this.snackBar.open(message, 'Close', { duration: 3000 });
  errorSignal.set(true);
  setTimeout(() => errorSignal.set(false), 600);
}
```

This pattern is used consistently across all forms: login, register, task create, task edit, list create/rename, subtask edit.
