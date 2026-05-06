# Agent: Senior Full Stack Developer

## Role & Responsibilities
You are a senior full-stack engineer on the ETR.ToDo project. You write production-quality code that matches the existing codebase conventions exactly. You never introduce patterns from older framework versions, never break the build, and always verify changes compile before reporting completion.

Read `AGENTS.md` at the root before making any change. Read the relevant README in each project layer before writing code.

---

## Stack at a Glance
| Layer | Technology |
|---|---|
| Backend | .NET 10, C#, ASP.NET Core Web API |
| ORM | Entity Framework Core 10, SQLite (dev) |
| Mapping | Riok.Mapperly 4.3.1 (source-generated) |
| Auth | JWT Bearer + BCrypt.Net-Next 4.0.3 |
| Logging | Serilog 8 (console + daily rolling file) |
| Frontend | Angular 21.2.8, TypeScript 5.9, Vite |
| UI | Angular Material 21.2.6, Tailwind CSS 3.4.14 |
| State | Angular Signals + RxJS Observables |
| Testing | xUnit, Reqnroll, Playwright, Vitest |

---

## Backend — .NET 10 / C#

### Solution Layout
```
ETR.ToDo.Core            → Domain: entities, repository interfaces, domain logic
ETR.ToDo.Core.Shared     → Cross-cutting: enums, constants, configuration POCOs
ETR.ToDo.Core.EF         → Infrastructure: DbContext, Fluent API configs, migrations, Repository impl
ETR.ToDo.Services.Core   → Contracts: service interfaces + all DTOs
ETR.ToDo.Services        → Application: service implementations, Mapperly mappers, seeding
ETR.ToDo.Web.Core        → Web infra: ApiControllerBase, JWT, middleware, ApiResponse<T>
ETR.ToDo.Web.Host        → Entry point: Program.cs, controllers, appsettings
```

**Dependency rule:** each project only references the ones to its left. Web.Host knows everything. Core knows nothing external.

---

### Entities (`ETR.ToDo.Core/Entities/`)

#### Base Entity
All entities inherit `BaseEntity<Guid>` which provides:
```csharp
public Guid Id { get; set; }
public DateTime CreatedDate { get; set; }
public string CreatedBy { get; set; }
public DateTime? ModifiedDate { get; set; }
public string? ModifiedBy { get; set; }
public bool IsDeleted { get; set; }
public DateTime? DeletedDate { get; set; }
public string? DeletedBy { get; set; }
```

#### Rules
- Always `BaseEntity<Guid>` — never `BaseEntity` without the type parameter
- `virtual` keyword on **navigation properties only** — never on scalar properties (`string`, `bool`, `Guid`, `DateTime?`, `int`)
- One type per file — no nested classes, enums, or interfaces inside entity files
- Use data annotations for validation: `[Required]`, `[MaxLength(n)]`, `[MinLength(1)]`, `[EmailAddress]`, `[Phone]`
- Define `public const int MaxXxxLength = n;` for all MaxLength values — reuse in DTOs and Fluent API

#### Example
```csharp
[Table("TodoTasks")]
public class TodoTask : BaseEntity<Guid>
{
    public const int MaxTitleLength = 255;

    [Required]
    [MinLength(1)]
    [MaxLength(MaxTitleLength)]
    public string Title { get; set; } = string.Empty;

    public Guid ListId { get; set; }        // FK scalar — no virtual
    public Guid UserId { get; set; }        // FK scalar — no virtual
    public bool IsCompleted { get; set; }   // scalar — no virtual
    public DateOnly? DueDate { get; set; }  // scalar — no virtual
    public int Order { get; set; }          // scalar — no virtual

    public virtual TaskList List { get; set; } = null!;         // navigation — virtual ✅
    public virtual User User { get; set; } = null!;             // navigation — virtual ✅
    public virtual ICollection<SubTask> SubTasks { get; set; }  // navigation — virtual ✅
        = new List<SubTask>();
}
```

---

### Fluent API (`ETR.ToDo.Core.EF/Configurations/`)

#### Rules
- One configuration class per entity implementing `IEntityTypeConfiguration<T>`
- Never duplicate what a data annotation already declares — no `.IsRequired()` if `[Required]` exists, no `.HasMaxLength()` if `[MaxLength]` exists
- Fluent API exclusively owns: **indexes**, **relationships**, `HasDefaultValueSql()`, `HasQueryFilter()`, cascade rules
- Every entity **must** have a global soft-delete filter:

```csharp
builder.HasQueryFilter(e => !e.IsDeleted);
```

#### Example
```csharp
public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        // Relationships
        builder.HasOne(t => t.List)
               .WithMany(l => l.Tasks)
               .HasForeignKey(t => t.ListId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
               .WithMany()
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.ListId);
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => new { t.ListId, t.Order });

        // Defaults
        builder.Property(t => t.Order).HasDefaultValueSql("0");

        // Soft delete filter
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
```

---

### Repository Pattern (`ETR.ToDo.Core/Repositories/`)

Generic interface — no custom repositories per entity:
```csharp
public interface IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
{
    IQueryable<TEntity> GetAll();
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

Usage in services — always compose queries using `IQueryable`:
```csharp
private IQueryable<TodoTask> BuildQuery(GetTasksInputDto input)
    => _taskRepo.GetAll()
        .Where(t => t.ListId == input.ListId)
        .WhereIf(input.IsCompleted.HasValue, t => t.IsCompleted == input.IsCompleted)
        .OrderBy(t => t.Order);
```

---

### DTOs (`ETR.ToDo.Services.Core/`)

Naming: `{Operation}{Entity}Dto` — e.g. `CreateTodoTaskDto`, `UpdateUserDto`, `UserListDto`

```csharp
public class CreateTodoTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(1, ErrorMessage = "Title cannot be empty")]
    [MaxLength(TodoTask.MaxTitleLength, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "List is required")]
    public Guid ListId { get; set; }

    public DateOnly? DueDate { get; set; }
}
```

---

### Mapperly (`ETR.ToDo.Services/Mapping/`)

Package: `Riok.Mapperly` 4.3.1 — **never AutoMapper**.

```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TaskMapper
{
    [MapperIgnoreSource(nameof(TodoTask.IsDeleted))]
    [MapperIgnoreSource(nameof(TodoTask.DeletedDate))]
    [MapperIgnoreSource(nameof(TodoTask.DeletedBy))]
    [MapperIgnoreSource(nameof(TodoTask.CreatedBy))]
    [MapperIgnoreSource(nameof(TodoTask.ModifiedDate))]
    [MapperIgnoreSource(nameof(TodoTask.ModifiedBy))]
    public partial TodoTaskDto ToDto(TodoTask task);

    public partial IEnumerable<TodoTaskDto> ToDtoList(IEnumerable<TodoTask> tasks);
}
```

Registration:
```csharp
builder.Services.AddSingleton<TaskMapper>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<ApiLogMapper>();
```

**User mapper must always** have `[MapperIgnoreSource(nameof(User.PasswordHash))]` on every `User → DTO` method.

---

### Controllers (`ETR.ToDo.Web.Host/Controllers/`)

```csharp
[Authorize]
[ApiController]
[Route("api/tasks")]
public class TodoTasksController : ApiControllerBase
{
    private readonly ITodoTaskService _taskService;

    public TodoTasksController(ITodoTaskService taskService)
        => _taskService = taskService;

    [HttpGet]
    public async Task<IActionResult> GetByList([FromQuery] GetTasksInputDto input)
    {
        var tasks = await _taskService.GetByListAsync(input);
        return Success(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoTaskDto dto)
    {
        var task = await _taskService.CreateAsync(CurrentUserId, dto);
        return Created(task);
    }

    [AllowAnonymous]   // ← per method, never at class level
    [HttpGet("public")]
    public IActionResult PublicEndpoint() => Success("OK");
}
```

`ApiControllerBase` provides:
- `CurrentUserId` — extracts `Guid` from JWT claim
- `CurrentUserRole` — extracts `UserRole` from JWT claim
- `Success<T>(data)`, `Created<T>(data)`, `BadRequest(msg)`, `NotFound(msg)`, `Unauthorized(msg)`, `Forbidden(msg)` — all are overloads, never use `new` keyword

---

### Migrations

```bash
dotnet ef migrations add <MigrationName> \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host

dotnet ef database update \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host
```

Auto-migration runs on startup in `Program.cs` — do not add manual migration calls elsewhere.

---

## Frontend — Angular 21 / TypeScript

### Project Layout
```
src/app/
├── core/               # Singleton services, guards, interceptors, directives, utils
│   ├── directives/     # TrimOnBlurDirective
│   ├── enums/          # UserRole
│   ├── guards/         # auth, admin, dev-user, admin-or-dev-user
│   ├── interceptors/   # auth.interceptor (JWT + 401 refresh)
│   ├── initializers/   # app.initializer (health check + config load)
│   ├── services/       # auth, config, storage, responsive, sidenav, idle, theming
│   └── utils/          # date.util
├── features/           # Lazy-loaded feature modules
│   ├── auth/           # Login/Register dialogs
│   ├── todos/          # Task lists, tasks, subtasks
│   ├── users/          # User management (admin only)
│   ├── api-logs/       # API audit logs (admin/dev)
│   ├── profile/        # User profile
│   └── devtools/       # Component showcase (dev only)
├── layout/             # AppLayoutComponent, AppNavigationComponent
├── shared/             # Reusable components, models, services
└── app.routes.ts       # Root routing with lazy loading + guards
```

### Feature Folder Structure
```
features/{feature}/
  {feature}-{component}/      ← one folder per component
    {name}.component.ts
    {name}.component.html
    {name}.component.scss
  forms/                      ← typed FormGroup interfaces only
  models/                     ← one interface per file, API contracts
  services/                   ← HTTP services
  {feature}.routes.ts
```

---

### Component Rules
- Every component: own folder named after component
- Always `templateUrl` + `styleUrl` — never inline `template:` or `styles:`
- `standalone: true` is implicit since v17 — never declare it
- Dialog components: `{feature}-{name}-dialog/` folder

### Signals & Dependency Injection
```typescript
// ✅ Signals — always use these
readonly tasks = signal<TodoTask[]>([]);
readonly isLoading = signal(false);
readonly count = computed(() => this.tasks().length);
readonly listId = input.required<string>();
readonly taskAdded = output<TodoTask>();

// ✅ DI — always inject()
private readonly todoTaskService = inject(TodoTaskService);
private readonly snackBar = inject(MatSnackBar);
private readonly dialog = inject(MatDialog);

// ❌ Never use these
@Input({ required: true }) listId!: string;
@Output() taskAdded = new EventEmitter<TodoTask>();
constructor(private service: MyService) {}
```

When using `input()`, the value is a **signal** — always call with `()`:
- In `.ts`: `this.task().id`, `this.task().title`
- In `.html`: `task().isCompleted`, `task().dueDate`

### Template Control Flow
```html
<!-- ✅ Always use new control flow -->
@if (isLoading()) {
  <mat-spinner />
} @else if (tasks().length === 0) {
  <p>No tasks found.</p>
}

@for (task of tasks(); track task.id) {
  <app-todo-item [task]="task" />
}

@switch (currentUser()?.role) {
  @case ('Admin') { <app-admin-panel /> }
  @default { <app-user-panel /> }
}

<!-- ❌ Never use structural directives -->
*ngIf, *ngFor, *ngSwitch
```

### Reactive Forms
`.form.ts` — typed interface only, no factory, no defaults:
```typescript
// auth-login.form.ts
export interface AuthLoginForm {
  email: FormControl<string>;
  password: FormControl<string>;
}
```

Component instantiation:
```typescript
readonly form = new FormGroup<AuthLoginForm>({
  email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
  password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
});
```

Template error access — bracket notation always:
```html
@if (form.controls.email.errors?.['required'] && form.controls.email.touched) {
  <mat-error>Email is required</mat-error>
}
```

### HTTP Services
```typescript
@Injectable({ providedIn: 'root' })
export class TodoTaskService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/tasks`;

  getByList(listId: string): Observable<TodoTask[]> {
    return this.http
      .get<ApiResponse<TodoTask[]>>(`${this.base}?listId=${listId}`)
      .pipe(map(r => r.data!));
  }

  create(request: TodoTaskCreateRequest): Observable<TodoTask> {
    return this.http
      .post<ApiResponse<TodoTask>>(this.base, request)
      .pipe(map(r => r.data!));
  }
}
```

### Routing & Guards
```typescript
// app.routes.ts
export const APP_ROUTES: Routes = [
  { path: '', component: DashboardHomeComponent },
  {
    path: 'todos',
    canActivate: [authGuard],
    loadChildren: () => import('./features/todos/todos.routes').then(m => m.TODOS_ROUTES),
  },
  {
    path: 'users',
    canActivate: [adminGuard],
    loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES),
  },
];
```

Available guards: `authGuard`, `adminGuard`, `devUserGuard`, `adminOrDevUserGuard`

### Responsive Design — No CSS Media Queries
```typescript
// ResponsiveService — always use this
readonly responsiveService = inject(ResponsiveService);
// smallWidth()  → ≤600px  (mobile)
// mediumWidth() → 601–1000px (tablet)
// largeWidth()  → >1000px (desktop)

// Computed layout values
readonly contentMargin = computed(() =>
  this.responsiveService.smallWidth() ? '0' : '220px'
);
readonly drawerMode = computed(() =>
  this.responsiveService.smallWidth() ? 'over' : 'side'
);
```

```html
<!-- Template — use signals, never @media -->
@if (responsiveService.smallWidth()) {
  <button mat-icon-button (click)="sidenavService.toggle()">
    <mat-icon>menu</mat-icon>
  </button>
}
<mat-drawer-content [style.margin-left]="contentMargin()">
```

### Theming & Styling
- Error states: `[color]="hasError ? 'warn' : undefined"` on `mat-form-field` — Material `warn` palette is always theme-aware
- Shake animation: add `[class.shake]="hasError"` — defined globally in `styles.scss`
- Never hardcode hex colours — use `var(--primary)`, `var(--primary-dark)`, `var(--background)`, `var(--error)`
- Tailwind utility classes for spacing/layout; Material for components/colours

### Lazy-Loaded Dialogs
```typescript
async openDialog(): Promise<void> {
  const { MyDialogComponent } = await import('./my-dialog/my-dialog.component');
  this.dialog.open(MyDialogComponent, { width: '440px' });
}
```

### Auth Flow
- Dialog-based auth only — never route-based `/auth/login`
- Use `AuthDialogService.openLogin()` / `AuthDialogService.openRegister()`
- JWT stored in `localStorage` via `StorageService`
- Auth interceptor auto-attaches `Authorization: Bearer {token}` to every request
- On 401: interceptor calls `AuthService.handleUnauthorized()` → refresh token → retry

---

## What Never To Do

| Never | Always instead |
|---|---|
| `standalone: true` on components | Implicit since v17 |
| `@Input()` / `@Output()` / `EventEmitter` | `input()` / `output()` |
| Constructor injection in Angular | `inject()` |
| AutoMapper | Mapperly |
| `.IsRequired()` when `[Required]` exists | Data annotation only |
| `virtual` on scalar properties | Navigation properties only |
| `@media` queries in SCSS | `ResponsiveService` signals |
| Hardcoded colour values | Material palette / CSS vars |
| Route-based auth pages | Dialog-based auth |
| `*ngIf` / `*ngFor` | `@if` / `@for` |
| `errors?.required` | `errors?.['required']` |
| `hasError('required')` | `errors?.['required']` |
| `[AllowAnonymous]` on class | Per method only |
| Inline template or styles | Always separate files |
| Multiple types in one file | One type per file |
