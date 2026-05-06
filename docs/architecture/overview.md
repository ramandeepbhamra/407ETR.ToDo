# Architecture Overview

## Backend — Clean Architecture

### Layer Diagram

```
┌──────────────────────────────────────────┐
│           ETR.ToDo.Web.Host               │  Controllers, Program.cs
├──────────────────────────────────────────┤
│           ETR.ToDo.Web.Core               │  ApiControllerBase, JWT, middleware
├──────────────────────────────────────────┤
│           ETR.ToDo.Services               │  Business logic, Mapperly mappers, seeder
├──────────────────────────────────────────┤
│        ETR.ToDo.Services.Core             │  Service interfaces, all DTOs
├──────────────────────────────────────────┤
│           ETR.ToDo.Core.EF                │  DbContext, Fluent API configs, migrations
├──────────────────────────────────────────┤
│            ETR.ToDo.Core                  │  Entities, IRepository<T,K>
├──────────────────────────────────────────┤
│         ETR.ToDo.Core.Shared              │  Enums, constants, config POCOs
└──────────────────────────────────────────┘
```

Inner layers never reference outer layers. `ETR.ToDo.Core` has zero external NuGet dependencies.

---

## Domain Model

```
User ──< TaskList    (TaskList.UserId → User.Id)
User ──< TodoTask    (TodoTask.UserId → User.Id)
User ──< RefreshToken
TaskList ──< TodoTask  (TodoTask.ListId → TaskList.Id)
TodoTask ──< SubTask   (SubTask.TodoTaskId → TodoTask.Id)
```

### BaseEntity\<Guid\>

All entities extend `BaseEntity<Guid>`, which provides:
- `Id` (GUID primary key)
- `CreatedDate`, `CreatedBy`, `ModifiedDate`, `ModifiedBy` — auto-populated by `ToDoDbContext.SaveChangesAsync()`
- `IsDeleted`, `DeletedDate`, `DeletedBy` — soft delete via global query filter

**Soft delete is mandatory.** Hard deletes are never used.

---

## Key Design Decisions

### Generic Repository
Single `IRepository<TEntity, TKey>` for all entities. Services inject `IRepository<TodoTask, Guid>` directly — no per-entity repository classes.

### Mapperly (Source-Generated Mapping)
All entity ↔ DTO mapping is done via Mapperly `partial` mapper classes. AutoMapper is not used. Mapping is compile-time with zero runtime reflection.

### Fluent API Only for EF Config
Entity configurations live in `IEntityTypeConfiguration<T>` classes in `ETR.ToDo.Core.EF/Configurations/`. Data annotations on entities are not duplicated in Fluent API.

### Exception-to-HTTP Mapping
Services throw typed exceptions. `ExceptionHandlingMiddleware` maps them to structured `ApiResponse` error responses:
- `KeyNotFoundException` → 404
- `UnauthorizedAccessException` → 403
- `InvalidOperationException` → 400

### Dialog-Based Auth
No `/auth/login` or `/auth/register` routes exist. Login and registration are Angular Material dialogs lazy-loaded via dynamic `import()`.

---

## Frontend — Angular 21 Standalone Signals

### Module Structure

```
src/app/
├── core/          Singletons: AuthService, ResponsiveService, SidenavService, guards, interceptors
├── features/      Lazy-loaded: auth, todos, users, api-logs, devtools, dashboard
├── layout/        AppLayoutComponent (shell)
└── shared/        Components and services used by ≥2 features
```

### State Management

| Concern | Mechanism |
|---|---|
| Component UI state | `signal()` |
| Derived state | `computed()` |
| Side effects | `effect()` |
| Shared app-wide state | Service-level `signal()` |
| HTTP | RxJS `Observable` → subscribe → `.set()` signal |
| Two-way binding | `model()` / `model.required()` |

### Responsive Layout

`ResponsiveService` (CDK BreakpointObserver) is the single source of screen-size truth:
- `smallWidth()` — ≤600px (mobile)
- `mediumWidth()` — 601–1000px (tablet)
- `largeWidth()` — >1000px (desktop)

CSS `@media` queries are never used.

`SidenavService` manages sidenav open/close state for both `TodoLayoutComponent` and `DevtoolsComponent`. On mobile, the sidenav is an overlay triggered by a hamburger in the top toolbar. `AppBottomNavComponent` provides role-aware navigation at the bottom on mobile.

### Authentication Flow

```
authInterceptor
  → attach "Authorization: Bearer {token}"
  → 401 response?
    → POST /api/auth/refresh-token
      → success: retry with new token
      → fail: logout + open login dialog
```

### App Initializer (Runs Before Bootstrap)

1. `GET /health` — shows non-dismissable dialog if API is unreachable
2. `GET /api/config` — populates `ConfigService` (idle timeout, support email)

### Theming

`ThemingService` writes CSS custom properties to `document.body`:
`--primary`, `--primary-light`, `--primary-dark`, `--background`, `--error`, `--ripple`

Preference is persisted in `localStorage`. Hex values are never hardcoded — always `var(--primary)` etc.

---

## API Design

### Response Envelope

```json
{ "success": true,  "data": { ... }, "message": null }
{ "success": false, "data": null,    "message": "Reason", "statusCode": 404 }
```

### Route Conventions

```
GET    /api/{resource}                  list / search
GET    /api/{resource}/{id}             get by id
POST   /api/{resource}                  create
PUT    /api/{resource}/{id}             full update
PATCH  /api/{resource}/{id}/{action}    partial action (complete, favourite)
DELETE /api/{resource}/{id}             soft delete
```

### Pagination

All list endpoints accept `PagedAndSortedRequestDto` and return `PagedResultDto<T>`.

---

## Security

### JWT + Refresh Token

- JWT access token: 60 min expiry, claims: `sub`, `email`, `role`, `exp`
- Refresh token: 7 days, single rotation (old token revoked on refresh)
- `MapInboundClaims = false` always set on `AddJwtBearer`

### Role Hierarchy

| Role | Access |
|---|---|
| Basic | Own task lists (≤10) and tasks per list (≤10) |
| Admin | User management, API logs |
| Dev | DevTools, API logs |

Limits are read from `appsettings.json → Application:Limits`. Never hardcoded.

---

## Observability

- **Health check:** `GET /health` — checks DB via `IHealthCheck`
- **Serilog:** Console sink + daily rolling file (`logs/log-YYYY-MM-DD.txt`)
- **ApiLoggingMiddleware:** Every request/response written to `ApiLogs` table, viewable at `GET /api/logs` (Admin + Dev only)
