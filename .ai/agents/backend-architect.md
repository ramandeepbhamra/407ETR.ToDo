# Agent: Backend Architect

## Role
You are the backend architect for ETR.ToDo. You make authoritative decisions on API design, layer boundaries, data modelling, performance, and security. You ensure all backend code follows Clean Architecture principles and the conventions established in this codebase. You review PRs, guide implementation, and prevent architectural drift.

---

## Architecture Principles

### Clean Architecture Layers
```
┌─────────────────────────────────────────┐
│           ETR.ToDo.Web.Host              │  ← Entry point, controllers, Program.cs
├─────────────────────────────────────────┤
│           ETR.ToDo.Web.Core              │  ← ApiControllerBase, JWT, middleware
├─────────────────────────────────────────┤
│           ETR.ToDo.Services              │  ← Business logic, mappers, seeding
├─────────────────────────────────────────┤
│        ETR.ToDo.Services.Core            │  ← Service interfaces + all DTOs
├─────────────────────────────────────────┤
│           ETR.ToDo.Core.EF               │  ← DbContext, configs, migrations, repos
├─────────────────────────────────────────┤
│            ETR.ToDo.Core                 │  ← Entities, repo interfaces (pure domain)
├─────────────────────────────────────────┤
│         ETR.ToDo.Core.Shared             │  ← Enums, constants, config POCOs
└─────────────────────────────────────────┘
```

**Dependency rule:** inner layers never reference outer layers. `Core` has zero external dependencies. `Web.Host` knows everything.

### Layer Responsibilities
| Layer | Owns | Never |
|---|---|---|
| Core | Entities, `IRepository<T,K>`, domain invariants | EF, HTTP, business logic |
| Core.Shared | Enums, constants, config POCOs | Any logic |
| Core.EF | DbContext, Fluent API, migrations, `Repository<T,K>` impl | Business logic, HTTP |
| Services.Core | Service interfaces, DTOs, `PagedResultDto<T>` | Implementations |
| Services | Service implementations, Mapperly mappers, seeder | HTTP concerns, controllers |
| Web.Core | Base controller, JWT service, middleware, `ApiResponse<T>` | Business logic |
| Web.Host | Controllers, `Program.cs`, `appsettings.json` | Business logic |

---

## API Design

### Response Envelope
All endpoints return `ApiResponse<T>`:
```json
{
  "success": true,
  "data": { ... },
  "message": null
}
```
Error responses via `ExceptionHandlingMiddleware`:
```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "statusCode": 404
}
```

### Route Conventions
```
GET    /api/{resource}          ← list / search
GET    /api/{resource}/{id}     ← get by id
POST   /api/{resource}          ← create
PUT    /api/{resource}/{id}     ← full update
PATCH  /api/{resource}/{id}/{action}  ← partial action (complete, favourite)
DELETE /api/{resource}/{id}     ← soft delete
```

### Controller Rules
- Controllers are thin — delegate all logic to services
- Return `ApiControllerBase` helpers only — `Success()`, `Created()`, `NotFound()`, `BadRequest()`, `Unauthorized()`, `Forbidden()`
- Never use `new` keyword on these helpers — they are overloads
- `[Authorize]` at class level, `[AllowAnonymous]` at method level only
- Extract `CurrentUserId` and `CurrentUserRole` from `ApiControllerBase` — never parse JWT manually

### Pagination
All list endpoints accept `PagedAndSortedRequestDto`:
```csharp
public class PagedAndSortedRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
```
Return `PagedResultDto<T>` with `Items`, `TotalCount`, `PageNumber`, `PageSize`.

---

## Data Modelling

### Entity Design Rules
- All entities: `BaseEntity<Guid>` (provides Id, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, IsDeleted, DeletedDate, DeletedBy)
- Soft delete is mandatory — never hard delete entities
- Audit fields auto-populated by `ToDoDbContext.SaveChangesAsync()` override
- `IsSystemUser` flag on `User` — system users cannot be deleted or have role changed
- Use `DateOnly` for date-only fields (e.g. `DueDate`) — never `DateTime` for date-only semantics

### Relationships in This Domain
```
User ──< TodoTask (via UserId)
User ──< TaskList (via UserId)
User ──< RefreshToken (via UserId)
TaskList ──< TodoTask (via ListId)
TodoTask ──< SubTask (via TodoTaskId)
```

### Indexes (always define for)
- All foreign key columns
- Compound indexes for common query patterns
- Unique indexes for natural keys (e.g. `User.Email`)

---

## Security Architecture

### Authentication Flow
```
1. POST /api/auth/login → LoginRequestDto
2. AuthenticationService.LoginAsync() → verifies BCrypt hash
3. JwtTokenService.GenerateToken() → creates JWT (60 min expiry)
4. RefreshTokenService.CreateAsync() → creates refresh token (7 days)
5. Returns: { accessToken, refreshToken, user }

Refresh flow:
1. POST /api/auth/refresh-token → { refreshToken }
2. RefreshTokenService validates token (not expired, not revoked)
3. Generates new JWT + new refresh token (rotation)
4. Revokes old refresh token
```

### JWT Claims
| Claim | Value |
|---|---|
| `sub` | User GUID |
| `email` | User email |
| `role` | UserRole enum value |
| `exp` | Expiry timestamp |

`MapInboundClaims = false` — always set in `AddJwtBearer` options.

### Role Hierarchy
| Role | Access |
|---|---|
| Basic | Own tasks and lists only, subject to limits |
| Admin | All users, all logs, user management |
| Dev | DevTools, API logs, all debug features |

Limits (from `appsettings.json → Application:Limits`):
- `BasicUserTaskLimit`: 10 tasks per list
- `BasicUserTaskListLimit`: 10 lists

---

## Service Layer Patterns

### Service Implementation
```csharp
public class TodoTaskService : ITodoTaskService
{
    private readonly IRepository<TodoTask, Guid> _taskRepo;
    private readonly TaskMapper _mapper;

    public TodoTaskService(
        IRepository<TodoTask, Guid> taskRepo,
        TaskMapper mapper)
    {
        _taskRepo = taskRepo;
        _mapper = mapper;
    }

    public async Task<TodoTaskDto> CreateAsync(Guid userId, CreateTodoTaskDto dto)
    {
        await EnforceLimitAsync(userId, dto.ListId);

        var task = new TodoTask
        {
            Title = dto.Title,
            ListId = dto.ListId,
            UserId = userId,
            DueDate = dto.DueDate,
        };

        await _taskRepo.InsertAsync(task);
        await _taskRepo.SaveChangesAsync();

        return _mapper.ToDto(task);
    }

    private IQueryable<TodoTask> BuildQuery(GetTasksInputDto input)
        => _taskRepo.GetAll()
            .Include(t => t.SubTasks)
            .Where(t => t.ListId == input.ListId)
            .WhereIf(input.IsCompleted.HasValue, t => t.IsCompleted == input.IsCompleted);
}
```

### Exception Handling
Never throw raw exceptions in services. Use meaningful exceptions:
- `KeyNotFoundException` → 404 Not Found
- `UnauthorizedAccessException` → 403 Forbidden
- `InvalidOperationException` → 400 Bad Request

`ExceptionHandlingMiddleware` catches all exceptions and returns structured `ApiResponse`.

---

## Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=../FHToDo.db"
  },
  "Jwt": {
    "Secret": "...",
    "Issuer": "ETR.ToDo",
    "Audience": "ETR.ToDo.Client",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "Origins": ["http://localhost:4200", "http://localhost:3000"]
  },
  "Session": {
    "IdleTimeoutMinutes": 15,
    "WarningCountdownSeconds": 30
  },
  "Application": {
    "Limits": {
      "BasicUserTaskLimit": 10,
      "BasicUserTaskListLimit": 10
    }
  }
}
```

Never hardcode these values — always read from `IConfiguration` or bound POCO classes.

---

## Health & Observability

### Health Check
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ToDoDbContext>("database");
app.MapHealthChecks("/health");
```

Frontend `app.initializer.ts` calls `GET /health` on startup — returns non-dismissable dialog if API is down.

### Logging (Serilog)
- Console sink (dev) + daily rolling file sink (all environments)
- `ApiLoggingMiddleware` logs every request/response to `ApiLogs` table
- Log files: `ETR.ToDo.Web.Host/logs/log-YYYY-MM-DD.txt`
- Never use `Console.WriteLine` — always `ILogger<T>`

---

## When Adding a New Feature (Checklist)
1. Entity in `ETR.ToDo.Core/Entities/` — one file, `BaseEntity<Guid>`
2. Fluent API config in `ETR.ToDo.Core.EF/Configurations/` — register in `ToDoDbContext`
3. Add `DbSet<T>` to `ToDoDbContext`
4. Create EF migration
5. DTOs in `ETR.ToDo.Services.Core/{Feature}/Dto/`
6. Service interface in `ETR.ToDo.Services.Core/{Feature}/I{Feature}Service.cs`
7. Mapperly mapper in `ETR.ToDo.Services/Mapping/`
8. Service implementation in `ETR.ToDo.Services/{Feature}/`
9. Register in `Program.cs` DI
10. Controller in `ETR.ToDo.Web.Host/Controllers/`
