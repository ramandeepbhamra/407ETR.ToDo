# GitHub Copilot Instructions — ETR.ToDo

You are an AI pair programmer on the ETR.ToDo project. Read this file in full before suggesting any code. These instructions override your defaults — follow them exactly.

---

## Project at a Glance

ETR.ToDo is a full-stack task management app:
- **Backend:** .NET 10, ASP.NET Core, EF Core 10, SQLite, Serilog, Mapperly, JWT + BCrypt
- **Frontend:** Angular 21 standalone components, Angular Material, Tailwind CSS (layout only)
- **Testing:** xUnit + Moq + FluentAssertions (BE unit), Reqnroll BDD (BE integration), Playwright (E2E), Vitest (FE unit)

Deep context files:
- `.ai/context/project-overview.md` — what the app does and who uses it
- `.ai/context/architecture.md` — layer diagram, entity design, routing, state management
- `.ai/context/coding-standards.md` — naming, patterns, forbidden practices
- `.ai/agents/backend-architect.md` — authoritative .NET patterns
- `.ai/agents/frontend-architect.md` — authoritative Angular patterns
- `.ai/agents/qa-engineer.md` — test patterns for all 4 layers

---

## Backend Rules (Non-Negotiable)

### Layers
```
Web.Host → Web.Core → Services → Services.Core → Core.EF → Core → Core.Shared
```
Inner layers never reference outer layers. `Core` has zero external dependencies.

### Entities
- Always extend `BaseEntity<Guid>`
- Soft delete only — never hard delete. `IsDeleted` is filtered via global query filter
- Use `DateOnly` for date-only fields — never `DateTime`
- Audit fields (`CreatedBy`, `ModifiedBy`, etc.) are auto-set by `ToDoDbContext.SaveChangesAsync()`

### Mapping
- **Mapperly only** — never AutoMapper, never manual `new XDto { Prop = entity.Prop }`
- Mapper classes: `partial` class decorated with `[Mapper]` in `ETR.ToDo.Services/Mapping/`

### Services
- Constructor injection (registered in `Program.cs`)
- Compose EF queries via `_repo.GetAll()` + LINQ — never raw SQL
- Exception semantics: `KeyNotFoundException` → 404, `UnauthorizedAccessException` → 403, `InvalidOperationException` → 400
- Never catch exceptions in services — let `ExceptionHandlingMiddleware` handle them

### Controllers
- Thin — one line delegating to service
- `[Authorize]` class-level; `[AllowAnonymous]` method-level only
- Use only `ApiControllerBase` helpers: `Success()`, `Created()`, `NotFound()`, `BadRequest()`, `Unauthorized()`, `Forbidden()`
- Extract user identity from `CurrentUserId` / `CurrentUserRole` — never parse JWT manually

### EF Config
- Fluent API only in `IEntityTypeConfiguration<T>` — no data annotations on entities
- Register in `ToDoDbContext.OnModelCreating()` via `modelBuilder.ApplyConfiguration()`

---

## Frontend Rules (Non-Negotiable)

### Angular Patterns
```typescript
// ✅ Always inject() — never constructor injection
private readonly myService = inject(MyService);

// ✅ input() / output() — never @Input() / @Output()
readonly title = input.required<string>();
readonly saved = output<void>();

// ✅ @if / @for — never *ngIf / *ngFor
// ✅ inject() not constructor
// ✅ No standalone: true (implicit)
// ✅ No NgModule
// ✅ Always separate .html and .scss files
```

### Signals
```typescript
readonly isLoading = signal(false);
readonly count = computed(() => this.items().length);

// Service-level shared state — readonly public signal
private readonly _user = signal<User | null>(null);
readonly user = this._user.asReadonly();
```

### HTTP
- Services return `Observable<T>` from HTTP calls
- Components subscribe and write to signals — never `toSignal()` with HTTP

### Dialogs
- Always lazy-load: `import('./path/dialog.component').then(m => this.dialog.open(m.DialogComponent, ...))`
- Auth dialogs are the canonical example — follow that pattern

### Responsive Layout
- **Never write `@media` queries in SCSS** — ever
- Use `ResponsiveService.smallWidth()` / `mediumWidth()` / `largeWidth()` in templates
- `SidenavService` (singleton) manages open/close state for all sidnavs

### Theming & Colours
- **Never hardcode hex, rgb, or colour names** in SCSS or templates
- Use CSS custom properties: `var(--primary)`, `var(--background)`, `var(--primary-light)`, etc.
- Material error states: `[color]="hasError() ? 'warn' : undefined"` — not custom red
- Tailwind for layout/spacing only — not for colours

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

### Auth
- No route `/auth/login` or `/auth/register` — ever
- Dialog-only auth via `AuthDialogService`

---

## Test Rules

### Backend Unit Tests (xUnit, `ETR.ToDo.Tests`)
- AAA pattern: Arrange / Act / Assert
- `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterised
- FluentAssertions always: `.Should().Be()`, `.Should().NotBeNull()`
- `Mock<IRepository<T,K>>` with MockQueryable for repo mocks

### BDD Integration Tests (Reqnroll, `ETR.ToDo.Tests.Api.BDD`)
- One `.feature` file per domain area
- Steps must be reusable — no step that can only be used in one scenario
- `CustomWebApplicationFactory` spins up real API + test SQLite DB — do not change infrastructure files

### E2E Tests (Playwright, `ETR.ToDo.Tests.Playwright`)
- Base URL: `http://localhost:4200`
- Selector priority: `#id` → `[data-testid]` → `[formControlName]` → ARIA role → text
- Never `page.waitForTimeout()` — use locator assertions instead
- Always `waitForLoadState('networkidle')` after navigation

### Frontend Unit Tests (Vitest, co-located `*.spec.ts`)
- `vi.fn()` for all dependencies
- `describe` → `beforeEach` → `it` structure
- Assert signals by calling them: `service.mySignal()`

---

## Forbidden Patterns — Never Suggest These

| Forbidden | Reason |
|---|---|
| AutoMapper | Project uses Mapperly |
| `@media` in SCSS | Use ResponsiveService |
| Hardcoded colours | Breaks theming |
| Route-based `/auth/login` | Dialog-only auth |
| `@Input()` / `@Output()` | Use `input()` / `output()` |
| Constructor injection (FE) | Use `inject()` |
| `*ngIf` / `*ngFor` | Use `@if` / `@for` |
| `standalone: true` | Implicit since Angular v17 |
| `NgModule` | App is fully standalone |
| Inline template/styles | Always separate files |
| Hard delete on any entity | Soft delete only |
| Raw SQL in services | Use `_repo.GetAll()` + LINQ |
| `try/catch` in services | Let ExceptionHandlingMiddleware handle |
| `Console.WriteLine` | Use `ILogger<T>` |
| Hardcoded session/limit values | Read from config |
| `errors?.required` in templates | `errors?.['required']` |
| Multiple exports per file | One type per file |
| Feature code in `core/` | Only singletons in `core/` |

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

## When Adding a New Feature — Checklist

**Backend:**
1. Entity → `ETR.ToDo.Core/Entities/` (extend `BaseEntity<Guid>`)
2. Fluent config → `ETR.ToDo.Core.EF/Configurations/` (register in `ToDoDbContext`)
3. Add `DbSet<T>` to `ToDoDbContext`
4. Add EF migration
5. DTOs → `ETR.ToDo.Services.Core/{Feature}/Dto/`
6. Service interface → `ETR.ToDo.Services.Core/{Feature}/`
7. Mapperly mapper → `ETR.ToDo.Services/Mapping/`
8. Service impl → `ETR.ToDo.Services/{Feature}/`
9. Register in `Program.cs`
10. Controller → `ETR.ToDo.Web.Host/Controllers/`

**Frontend:**
1. Model interfaces → `features/{feature}/models/`
2. Service → `features/{feature}/services/`
3. Component + template + styles → `features/{feature}/{component-name}/`
4. Add route to `{feature}.routes.ts`
5. If dialog: lazy-load via dynamic `import()`
6. If shared (≥2 features): move to `shared/`
