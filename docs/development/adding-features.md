# Adding New Features

Follow this checklist end-to-end when adding a new domain feature. Each step references the correct project and naming convention.

---

## Backend Checklist

### 1. Entity — `ETR.ToDo.Core/Entities/{Feature}/`

```csharp
public class MyEntity : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
```

Rules:
- Always extend `BaseEntity<Guid>`
- Use `DateOnly` for date-only fields — never `DateTime`
- Navigation properties are `virtual` only when lazy loading is explicitly required
- No data annotations — constraints go in Fluent API config

---

### 2. Fluent API Config — `ETR.ToDo.Core.EF/Configurations/`

```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("MyEntities");
        builder.HasIndex(e => e.UserId);
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
```

Register in `ToDoDbContext.OnModelCreating()`:
```csharp
modelBuilder.ApplyConfiguration(new MyEntityConfiguration());
```

---

### 3. DbSet — `ETR.ToDo.Core.EF/Context/ToDoDbContext.cs`

```csharp
public DbSet<MyEntity> MyEntities => Set<MyEntity>();
```

---

### 4. EF Migration

```bash
dotnet ef migrations add Add{MyEntity} \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host
```

Migration is applied automatically on next app startup via `MigrateAsync()`.

---

### 5. DTOs — `ETR.ToDo.Services.Core/{Feature}/Dto/`

```csharp
// Response DTO
public class MyEntityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

// Create DTO
public class CreateMyEntityDto
{
    public string Title { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

// Update DTO
public class UpdateMyEntityDto
{
    public string Title { get; set; } = string.Empty;
}
```

Rules:
- One file per DTO class — never bundle
- Response DTOs never expose `IsDeleted`, `PasswordHash`, or other internal fields
- List inputs extend `PagedAndSortedRequestDto`

---

### 6. Service Interface — `ETR.ToDo.Services.Core/{Feature}/`

```csharp
public interface IMyEntityService
{
    Task<PagedResultDto<MyEntityDto>> GetAllAsync(PagedAndSortedRequestDto input);
    Task<MyEntityDto> GetByIdAsync(Guid id);
    Task<MyEntityDto> CreateAsync(Guid userId, CreateMyEntityDto dto);
    Task<MyEntityDto> UpdateAsync(Guid id, Guid userId, UpdateMyEntityDto dto);
    Task DeleteAsync(Guid id, Guid userId);
}
```

---

### 7. Mapperly Mapper — `ETR.ToDo.Services/Mapping/`

```csharp
[Mapper]
public partial class MyEntityMapper
{
    public partial MyEntityDto ToDto(MyEntity entity);
    public partial MyEntity ToEntity(CreateMyEntityDto dto);
}
```

Never write manual `new MyEntityDto { Prop = entity.Prop }` — always Mapperly.

---

### 8. Service Implementation — `ETR.ToDo.Services/{Feature}/`

```csharp
public class MyEntityService : IMyEntityService
{
    private readonly IRepository<MyEntity, Guid> _repo;
    private readonly MyEntityMapper _mapper;

    public MyEntityService(IRepository<MyEntity, Guid> repo, MyEntityMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<MyEntityDto> CreateAsync(Guid userId, CreateMyEntityDto dto)
    {
        var entity = _mapper.ToEntity(dto);
        entity.UserId = userId;

        await _repo.InsertAsync(entity);
        await _repo.SaveChangesAsync();

        return _mapper.ToDto(entity);
    }

    private IQueryable<MyEntity> BuildQuery(Guid userId)
        => _repo.GetAll().Where(e => e.UserId == userId);
}
```

Exception semantics — never catch in services:
- `KeyNotFoundException` → 404
- `UnauthorizedAccessException` → 403
- `InvalidOperationException` → 400 (e.g. limit exceeded)

---

### 9. DI Registration — `ETR.ToDo.Web.Host/Program.cs`

```csharp
builder.Services.AddScoped<IMyEntityService, MyEntityService>();
builder.Services.AddSingleton<MyEntityMapper>();
```

---

### 10. Controller — `ETR.ToDo.Web.Host/Controllers/`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MyEntitiesController : ApiControllerBase
{
    private readonly IMyEntityService _service;

    public MyEntitiesController(IMyEntityService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedAndSortedRequestDto input)
    {
        var result = await _service.GetAllAsync(input);
        return Success(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMyEntityDto dto)
    {
        var result = await _service.CreateAsync(CurrentUserId, dto);
        return Created(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, CurrentUserId);
        return Success();
    }
}
```

Rules:
- Controllers are thin — one line delegating to service
- Use only `ApiControllerBase` helpers (`Success`, `Created`, `NotFound`, `BadRequest`, `Forbidden`, `Unauthorized`)
- Use `CurrentUserId` / `CurrentUserRole` — never parse JWT manually
- `[Authorize]` at class level, `[AllowAnonymous]` at method level only

---

## Frontend Checklist

### 1. Models — `features/{feature}/models/`

```typescript
// my-entity.model.ts
export interface MyEntity {
  id: string;
  title: string;
  userId: string;
}

// my-entity-create-request.model.ts
export interface MyEntityCreateRequest {
  title: string;
  listId: string;
}
```

One interface per file. File naming: `{feature}-{entity}-{operation}.model.ts`.

---

### 2. Service — `features/{feature}/services/`

```typescript
// my-entity.service.ts
@Injectable({ providedIn: 'root' })
export class MyEntityService {
  private readonly http = inject(HttpClient);

  getAll(listId: string): Observable<MyEntity[]> {
    return this.http
      .get<ApiResponse<MyEntity[]>>(`/api/myentities?listId=${listId}`)
      .pipe(map(r => r.data!));
  }

  create(dto: MyEntityCreateRequest): Observable<MyEntity> {
    return this.http
      .post<ApiResponse<MyEntity>>('/api/myentities', dto)
      .pipe(map(r => r.data!));
  }
}
```

Services return `Observable<T>` — never async/await, never `toSignal()` for HTTP.

---

### 3. Component — `features/{feature}/{component-name}/`

Three files always — never inline template or styles:
- `my-entity-list.component.ts`
- `my-entity-list.component.html`
- `my-entity-list.component.scss`

```typescript
// my-entity-list.component.ts
@Component({
  selector: 'app-my-entity-list',
  templateUrl: './my-entity-list.component.html',
  styleUrl: './my-entity-list.component.scss',
  imports: [/* Angular Material, CommonModule etc */],
})
export class MyEntityListComponent {
  private readonly service = inject(MyEntityService);
  private readonly snackBar = inject(MatSnackBar);

  readonly items = signal<MyEntity[]>([]);
  readonly isLoading = signal(false);
  readonly titleError = signal(false);

  loadItems(listId: string): void {
    this.isLoading.set(true);
    this.service.getAll(listId).subscribe({
      next: items => { this.items.set(items); this.isLoading.set(false); },
      error: err => {
        this.isLoading.set(false);
        this.snackBar.open(err.error?.message ?? 'Failed to load', 'Close', { duration: 3000 });
      },
    });
  }

  private triggerError(sig: WritableSignal<boolean>, msg: string): void {
    this.snackBar.open(msg, 'Close', { duration: 3000 });
    sig.set(true);
    setTimeout(() => sig.set(false), 600);
  }
}
```

Forbidden:
- `constructor` injection — use `inject()`
- `@Input()` / `@Output()` — use `input()` / `output()`
- `*ngIf` / `*ngFor` — use `@if` / `@for`
- `standalone: true` — implicit since Angular v17

---

### 4. Dialog (If Needed)

Dialogs are always lazy-loaded from the caller:

```typescript
openDialog(entity?: MyEntity): void {
  import('../my-entity-dialog/my-entity-dialog.component').then(m => {
    this.dialog.open(m.MyEntityDialogComponent, {
      width: '440px',
      data: { entity },
    });
  });
}
```

Dialog component pattern:

```typescript
@Component({ ... })
export class MyEntityDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<MyEntityDialogComponent>);
  readonly data = inject<{ entity?: MyEntity }>(MAT_DIALOG_DATA);
  readonly isEditMode = !!this.data.entity;

  submit(): void {
    // validate → call service → dialogRef.close(result)
  }
}
```

---

### 5. Route — `features/{feature}/{feature}.routes.ts`

```typescript
export const MY_FEATURE_ROUTES: Routes = [
  {
    path: '',
    component: MyFeatureLayoutComponent,
    children: [
      { path: '', component: MyEntityListComponent },
      { path: ':id', component: MyEntityDetailComponent },
    ],
  },
];
```

Register in `app.routes.ts` with the appropriate guard.

---

### 6. Validation (All Forms)

```typescript
readonly titleError = signal(false);

private triggerError(sig: WritableSignal<boolean>, msg: string): void {
  this.snackBar.open(msg, 'Close', { duration: 3000 });
  sig.set(true);
  setTimeout(() => sig.set(false), 600);
}
```

```html
<mat-form-field
  [color]="titleError() ? 'warn' : undefined"
  [class.shake]="titleError()"
>
  <mat-label>Title</mat-label>
  <input matInput [(ngModel)]="title" maxlength="255" />
</mat-form-field>
```

`.shake` and `@keyframes shake` are already defined globally in `styles.scss`.

---

## Naming Quick Reference

| Artefact | Pattern | Example |
|---|---|---|
| Entity | PascalCase | `TodoTask` |
| DTO | `{Action}{Entity}Dto` | `CreateTodoTaskDto`, `TodoTaskDto` |
| Service interface | `I{Feature}Service` | `ITodoTaskService` |
| Mapperly mapper | `{Feature}Mapper` | `TaskMapper` |
| Controller | `{Feature}Controller` | `TodoTasksController` |
| FE model | `{feature}-{entity}-{op}.model.ts` | `todo-task-create-request.model.ts` |
| FE service | `{feature}-{entity}.service.ts` | `todo-task.service.ts` |
| FE component | `{feature}-{entity}-{action}.component.ts` | `todo-task-list.component.ts` |
| FE dialog | `{feature}-{entity}-dialog.component.ts` | `todo-task-list-dialog.component.ts` |
