# ETR.ToDo.Web.Host — API Entry Point

ASP.NET Core 10 Web API. Thin controllers + Program.cs DI configuration.

---

## What Lives Here

```
ETR.ToDo.Web.Host/
├── Controllers/
│   ├── AuthController.cs         login, register, refresh, revoke
│   ├── ConfigController.cs       GET /api/config (public)
│   ├── ProfileController.cs      GET/PUT /api/profile (authenticated)
│   ├── UsersController.cs        user CRUD (Admin only)
│   ├── TaskListsController.cs    task list CRUD
│   ├── TodoTasksController.cs    task CRUD + subtask endpoints
│   └── ApiLogsController.cs      API log viewer (Admin + Dev)
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Controller Rules

- Controllers are thin — delegate all logic to services
- `[Authorize]` at class level; `[AllowAnonymous]` at method level only
- Return only `ApiControllerBase` helpers: `Success()`, `Created()`, `NotFound()`, `BadRequest()`, `Unauthorized()`, `Forbidden()`
- Use `CurrentUserId` / `CurrentUserRole` — never parse JWT manually

---

## Key Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | Anonymous | Login → JWT + refresh token |
| POST | `/api/auth/register` | Anonymous | Self-registration |
| POST | `/api/auth/refresh-token` | Anonymous | Rotate refresh token |
| POST | `/api/auth/revoke` | Authorized | Revoke refresh token (logout) |
| GET | `/api/config` | Anonymous | Session config for frontend |
| GET | `/api/profile` | Authorized | Current user profile |
| PUT | `/api/profile` | Authorized | Update own profile |
| GET | `/api/users` | Admin | Paginated user list |
| GET | `/api/tasklists` | Authorized | User's task lists |
| GET | `/api/todotasks` | Authorized | Tasks by list |
| GET | `/api/logs` | Admin + Dev | API log viewer |
| GET | `/health` | Anonymous | DB health check |

---

## Configuration

```json
{
  "ConnectionStrings": { "DefaultConnection": "Data Source=../ETRToDo.db" },
  "Jwt": {
    "Secret": "...",
    "Issuer": "ETR.ToDo",
    "Audience": "ETR.ToDo.Client",
    "ExpirationMinutes": 60
  },
  "Cors": { "Origins": ["http://localhost:4200"] },
  "Session": { "IdleTimeoutMinutes": 15, "WarningCountdownSeconds": 30 },
  "Application": {
    "Limits": { "BasicUserTaskLimit": 10, "BasicUserTaskListLimit": 10 }
  }
}
```

Never hardcode any of these values — always read from `IConfiguration`.

---

## DI Registration Pattern

```csharp
// Generic repository (covers all entities)
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

// Mapperly mappers (stateless — singleton)
builder.Services.AddSingleton<TaskMapper>();
builder.Services.AddSingleton<UserMapper>();

// Application services
builder.Services.AddScoped<ITodoTaskService, TodoTaskService>();
builder.Services.AddScoped<IUserService, UserService>();
```

---

## Startup Sequence

`Program.cs` runs in this order before accepting requests:

1. Apply EF migrations (`MigrateAsync`)
2. Seed system users (`SeedAsync`)
3. Map health check endpoint (`/health`)
4. Start Kestrel
