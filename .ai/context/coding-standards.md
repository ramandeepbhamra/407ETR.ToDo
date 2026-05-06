# Coding Standards — ETR.ToDo

## Universal Rules (Frontend + Backend)

- One type per file — no bundling multiple classes or interfaces in a single file
- No hardcoded configuration values — always read from `appsettings.json` (BE) or `ConfigService` (FE)
- No raw exception throwing — use `KeyNotFoundException`, `UnauthorizedAccessException`, `InvalidOperationException` (BE); let `ExceptionHandlingMiddleware` handle them
- No `Console.WriteLine` — always `ILogger<T>` (BE) or browser console only in dev builds (FE)
- Soft delete only — never `DELETE` SQL or `_repo.DeleteAsync()` for permanent removal
- Comments: only when the **why** is non-obvious. Never explain what the code does.

---

## Backend Standards (.NET 10)

### Naming
| Artefact | Convention | Example |
|---|---|---|
| Entity | PascalCase noun | `TodoTask`, `TaskList` |
| DTO | PascalCase + `Dto` suffix | `TodoTaskDto`, `CreateTodoTaskDto` |
| Service interface | `I` prefix + `Service` suffix | `ITodoTaskService` |
| Service impl | No prefix, `Service` suffix | `TodoTaskService` |
| Mapper | `{Feature}Mapper` | `TaskMapper` |
| Controller | `{Feature}Controller` | `TodoTasksController` |
| Repository | `IRepository<T,K>` (generic) | — |
| Migration | Descriptive past-tense verb | `AddSubTaskTable` |

### Entity Rules
- Always extend `BaseEntity<Guid>`
- Use `DateOnly` for date-only fields (e.g. `DueDate`) — never `DateTime`
- Foreign key column: `{EntityName}Id` (e.g. `ListId`, `UserId`)
- Index every FK and every compound query column
- Unique index on natural keys (e.g. `User.Email`)

### Fluent API (EF Core)
- All configuration in `IEntityTypeConfiguration<T>` classes in `ETR.ToDo.Core.EF/Configurations/`
- Never duplicate data annotations on entities — Fluent API is the single source of truth
- Register configs in `ToDoDbContext.OnModelCreating()` via `modelBuilder.ApplyConfiguration(new XConfig())`
- `IsDeleted` filter: `modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted)`

### DTO Rules
- Create-specific DTO: `CreateXDto` — never reuse the entity or the response DTO
- Update-specific DTO: `UpdateXDto`
- Response DTO: `XDto` — only what the client needs
- Paged list input: extend `PagedAndSortedRequestDto`
- Paged list output: wrap in `PagedResultDto<T>`

### Mapperly
```csharp
// Always partial, always in ETR.ToDo.Services/Mapping/
[Mapper]
public partial class TaskMapper
{
    public partial TodoTaskDto ToDto(TodoTask entity);
    public partial TodoTask ToEntity(CreateTodoTaskDto dto);
}
```
Never write manual `new XDto { Prop = entity.Prop }` mapping. If Mapperly can't infer a mapping, use `[MapProperty("Source", "Target")]`.

### Controller Rules
- Thin — delegate everything to the service
- `[Authorize]` at class level; `[AllowAnonymous]` only at method level
- Extract identity from `ApiControllerBase.CurrentUserId` / `CurrentUserRole` — never parse JWT claims manually
- Return only `ApiControllerBase` helper methods: `Success()`, `Created()`, `NotFound()`, `BadRequest()`, `Unauthorized()`, `Forbidden()`

### Service Rules
- Constructor injection (DI registered in `Program.cs`)
- Compose queries with `_repo.GetAll()` + LINQ — never raw SQL
- Use `.WhereIf(condition, predicate)` extension for optional filters
- Enforce role-based limits before inserting; throw `InvalidOperationException` with a descriptive message
- Always `await _repo.SaveChangesAsync()` after mutations

---

## Frontend Standards (Angular 21)

### Naming
| Artefact | Convention | Example |
|---|---|---|
| Component | `{feature}-{entity}-{action}.component.ts` | `todo-task-list.component.ts` |
| Dialog | `{feature}-{entity}-dialog.component.ts` | `todo-task-list-dialog.component.ts` |
| Service | `{feature}-{entity}.service.ts` | `todo-task.service.ts` |
| Model/DTO | `{feature}-{entity}-{operation}.model.ts` | `todo-task-create-request.model.ts` |
| Form interface | `{feature}-{entity}.form.ts` | `auth-login.form.ts` |
| Guard | `{action}Guard` camelCase | `authGuard`, `adminGuard` |
| Enum | PascalCase | `UserRole` |

### Component Rules
```typescript
// ✅ Correct
@Component({ selector: 'app-...', templateUrl: '...', styleUrl: '...' })
export class MyComponent {
  private readonly myService = inject(MyService);  // inject(), not constructor
  readonly items = signal<Item[]>([]);
  readonly count = computed(() => this.items().length);
}
```

| Forbidden | Use instead |
|---|---|
| `constructor(private svc: Svc)` | `inject(Svc)` |
| `@Input()` / `@Output()` | `input()` / `output()` |
| `*ngIf` / `*ngFor` | `@if` / `@for` |
| `standalone: true` | Omit (implicit since v17) |
| `NgModule` | Not used — fully standalone |
| Inline `template:` / `styles:` | Always separate `.html` / `.scss` files |
| `errors?.required` | `errors?.['required']` |

### Signal Patterns
```typescript
// Writable
readonly isLoading = signal(false);

// Read-only public, writable private
private readonly _user = signal<User | null>(null);
readonly user = this._user.asReadonly();

// Derived
readonly isAdmin = computed(() => this.user()?.role === UserRole.Admin);

// Two-way binding
readonly isOpen = model.required<boolean>();

// Effect
constructor() {
  effect(() => { this.isOpen.set(!this.responsiveService.smallWidth()); });
}
```

### HTTP Pattern
Services return `Observable<T>`. Components subscribe and write to signals:
```typescript
// Service
getAll(): Observable<TodoTask[]> {
  return this.http.get<ApiResponse<TodoTask[]>>('/api/tasks').pipe(map(r => r.data!));
}

// Component
this.taskService.getAll().subscribe({
  next: tasks => this.tasks.set(tasks),
  error: err => this.snackBar.open(err.error?.message ?? 'Error', 'Close', { duration: 3000 }),
});
```
Never use `toSignal()` with HTTP calls. Never `async/await` in components.

### Dialog Pattern
Always lazy-load dialogs:
```typescript
openDialog(): void {
  import('./my-feature-dialog/my-feature-dialog.component').then(m => {
    this.dialog.open(m.MyFeatureDialogComponent, { width: '440px', data: { ... } });
  });
}
```

Dialog component:
```typescript
export class MyFeatureDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<MyFeatureDialogComponent>);
  readonly data = inject<MyDialogData>(MAT_DIALOG_DATA);
}
```

### Error UX (mandatory for all forms)
```typescript
readonly titleError = signal(false);

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
The `.shake` class and `@keyframes shake` are defined globally in `styles.scss`.

### Responsive — Non-Negotiable Rules
- **Never write `@media` in any SCSS file** — use `ResponsiveService`
- Read `responsiveService.smallWidth()` / `mediumWidth()` / `largeWidth()` in templates and `computed()`
- Sidenav behaviour (overlay vs. side) driven by `SidenavService.isOpen` signal
- Bottom nav visibility: `@if (responsiveService.smallWidth() && isAuthenticated())`

### Theming — Non-Negotiable Rules
- Never hardcode hex, rgb, or colour names in SCSS or templates
- Brand colours: `var(--primary)`, `var(--primary-light)`, `var(--primary-dark)`, `var(--background)`, `var(--error)`
- Material `warn` palette for error states — not custom red
- Tailwind for layout and spacing only (padding, margin, flex, grid) — not for colours

### Validation Lengths (entity-driven)
| Field | Min | Max |
|---|---|---|
| Task title | 1 | 255 |
| Subtask title | 1 | 255 |
| Task list name | 1 | 100 |
| User display name | 1 | 100 |
| Email | — | 256 |
| Password | 8 | — |

---

## Git Conventions

### Branches
- Feature: `feature/{short-description}`
- Bug fix: `fix/{short-description}`
- Docs: `docs/{short-description}`

### Commit Messages
- Imperative mood, present tense: "Add due date to task form"
- No trailing period
- Reference ticket if applicable: "Fix sidenav overlay on mobile (#42)"

### What Never Gets Committed
- `*.db`, `*.db-shm`, `*.db-wal` (SQLite files)
- `appsettings.Production.json`
- `node_modules/`
- Angular build output (`dist/`)
- `.NET` publish output

---

## File Organisation

### One Type Per File
Each `.ts`, `.cs` file exports exactly one class, interface, enum, or function set.

### Feature Folder Ownership
All code for a feature lives under `features/{feature-name}/`. Cross-feature code goes to `shared/` (if used by ≥2 features) or `core/` (if app-wide singleton). Never put feature-specific code in `core/`.

### Test Co-location (Frontend)
Vitest spec files live beside the file they test:
```
todo-task.service.ts
todo-task.service.spec.ts
```

### Test Projects (Backend)
Separate csproj per test type:
- `ETR.ToDo.Tests` — xUnit unit tests
- `ETR.ToDo.Tests.Api.BDD` — Reqnroll integration tests
- `ETR.ToDo.Tests.Playwright` — Playwright E2E (TypeScript)
