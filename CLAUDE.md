# CLAUDE.md — ETR.ToDo

You are an AI pair programmer on the ETR.ToDo project. Read this file in full before making any changes. These instructions override your defaults.

---

## Collaboration Rules

- **Always ask before making changes** — propose what you intend to do and wait for confirmation
- **Never create `.md` documentation files** unless explicitly asked
- **One type per file** — no bundling multiple classes or interfaces
- **No comments** unless the WHY is non-obvious (a hidden constraint, a workaround, a subtle invariant)
- **No cleanup beyond the task** — fix what was asked, nothing more

---

## Deep Context

Read these files before working on any area of the codebase:

- [.ai/context/project-overview.md](.ai/context/project-overview.md) — what the app does, roles, feature areas, tech stack
- [.ai/context/architecture.md](.ai/context/architecture.md) — layer diagram, entity design, routing, state, auth flow
- [.ai/context/coding-standards.md](.ai/context/coding-standards.md) — naming, patterns, forbidden practices, validation lengths
- [.ai/agents/backend-architect.md](.ai/agents/backend-architect.md) — authoritative .NET patterns
- [.ai/agents/frontend-architect.md](.ai/agents/frontend-architect.md) — authoritative Angular 21 patterns
- [.ai/agents/qa-engineer.md](.ai/agents/qa-engineer.md) — test patterns for all 4 layers
- [.ai/agents/devops-engineer.md](.ai/agents/devops-engineer.md) — build, migrations, config, observability

---

## Tech Stack

| Layer | Technology |
|---|---|
| API | .NET 10, ASP.NET Core, EF Core 10, SQLite |
| Mapping | Mapperly 4.3.1 (source-generated) — never AutoMapper |
| Auth | BCrypt, JWT (60 min), refresh token rotation (7 days) |
| Logging | Serilog — console + rolling file + `ApiLogs` DB table |
| Frontend | Angular 21, standalone components, signals |
| UI | Angular Material 21 + Tailwind CSS (layout/spacing only) |
| State | Angular signals — no NgRx, no BehaviorSubject |
| BE Tests | xUnit + Moq + FluentAssertions + MockQueryable |
| BDD Tests | Reqnroll + WebApplicationFactory |
| E2E Tests | Playwright (TypeScript, Chromium) |
| FE Tests | Vitest + @vitest/coverage-v8 |

---

## Backend — Critical Rules

- Clean Architecture: inner layers never reference outer layers
- Every entity extends `BaseEntity<Guid>` — audit fields auto-populated by `ToDoDbContext`
- **Soft delete only** — never hard delete any entity
- Use `DateOnly` for date-only fields — never `DateTime`
- **Mapperly only** for entity ↔ DTO mapping — no manual property assignment
- Fluent API only in `IEntityTypeConfiguration<T>` — no data annotations on entities
- Controllers are thin — delegate all logic to services
- Exception semantics: `KeyNotFoundException` → 404, `UnauthorizedAccessException` → 403, `InvalidOperationException` → 400
- Never `try/catch` in services — `ExceptionHandlingMiddleware` handles all exceptions
- Never parse JWT manually — use `ApiControllerBase.CurrentUserId` / `CurrentUserRole`
- Never `Console.WriteLine` — always `ILogger<T>`

---

## Frontend — Critical Rules

- `inject()` for DI — never constructor injection
- `input()` / `output()` — never `@Input()` / `@Output()`
- `@if` / `@for` — never `*ngIf` / `*ngFor`
- No `standalone: true` (implicit since Angular v17)
- No `NgModule`
- Always separate `.html` and `.scss` files — no inline template or styles
- Services return `Observable<T>` — components subscribe and write to signals
- Never `toSignal()` with HTTP calls
- **Always lazy-load dialogs** via dynamic `import()`
- **No route-based auth** — `/auth/login` does not exist; login and register are dialogs only

### Responsive (Non-Negotiable)
- **Never write `@media` in any SCSS file** — use `ResponsiveService` signals
- `SidenavService` (singleton) manages sidenav open/close state for all layouts

### Theming (Non-Negotiable)
- **Never hardcode hex, rgb, or colour names** in SCSS or templates
- Use CSS custom properties: `var(--primary)`, `var(--background)`, `var(--primary-light)`, `var(--primary-dark)`, `var(--error)`
- Material error states: `[color]="hasError() ? 'warn' : undefined"` — not custom red
- Tailwind for layout/spacing only — never for colours

### Error UX Pattern (All Forms)
```typescript
readonly titleError = signal(false);
private triggerError(sig: WritableSignal<boolean>, msg: string): void {
  this.snackBar.open(msg, 'Close', { duration: 3000 });
  sig.set(true);
  setTimeout(() => sig.set(false), 600);
}
```
```html
<mat-form-field [color]="titleError() ? 'warn' : undefined" [class.shake]="titleError()">
```
`.shake` and `@keyframes shake` are defined globally in `styles.scss`.

---

## Forbidden Patterns

| Forbidden | Use instead |
|---|---|
| AutoMapper | Mapperly |
| `@media` in SCSS | `ResponsiveService` |
| Hardcoded colours | CSS custom properties |
| Route-based `/auth/login` | Dialog-only auth |
| `@Input()` / `@Output()` | `input()` / `output()` |
| Constructor injection (FE) | `inject()` |
| `*ngIf` / `*ngFor` | `@if` / `@for` |
| `standalone: true` | Omit (implicit) |
| `NgModule` | Not used |
| Inline template/styles | Separate `.html` / `.scss` |
| Hard delete | Soft delete only |
| Raw SQL in services | `_repo.GetAll()` + LINQ |
| `try/catch` in services | `ExceptionHandlingMiddleware` |
| `Console.WriteLine` | `ILogger<T>` |
| Hardcoded config values | `appsettings.json` / `ConfigService` |
| `errors?.required` | `errors?.['required']` |
| Multiple exports per file | One type per file |
| Feature code in `core/` | `core/` is singletons only |

---

## Validation Field Lengths

| Field | Min | Max |
|---|---|---|
| Task / subtask title | 1 | 255 |
| Task list name | 1 | 100 |
| User name | 1 | 100 |
| Email | — | 256 |
| Password | 8 | — |

---

## New Feature Checklist

### Backend
1. Entity → `ETR.ToDo.Core/Entities/` (extend `BaseEntity<Guid>`)
2. Fluent config → `ETR.ToDo.Core.EF/Configurations/` (register in `ToDoDbContext`)
3. `DbSet<T>` → `ToDoDbContext`
4. EF migration
5. DTOs → `ETR.ToDo.Services.Core/{Feature}/Dto/`
6. Service interface → `ETR.ToDo.Services.Core/{Feature}/`
7. Mapperly mapper → `ETR.ToDo.Services/Mapping/`
8. Service impl → `ETR.ToDo.Services/{Feature}/`
9. Register in `Program.cs`
10. Controller → `ETR.ToDo.Web.Host/Controllers/`

### Frontend
1. Model interfaces → `features/{feature}/models/`
2. Service → `features/{feature}/services/`
3. Component + template + styles → `features/{feature}/{component-name}/`
4. Route → `{feature}.routes.ts`
5. If dialog: lazy-load via dynamic `import()`
6. If shared (≥2 features): move to `shared/`
