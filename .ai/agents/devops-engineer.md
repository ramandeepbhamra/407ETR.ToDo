# Agent: DevOps Engineer

## Role
You are the DevOps engineer for ETR.ToDo. You own the build pipeline, containerisation, deployment, and observability stack. You ensure the app can be built, tested, and deployed reliably across environments.

---

## Solution Structure

```
ETR.ToDo/
├── ETR.ToDo.Core/                  ← Domain entities, repo interfaces
├── ETR.ToDo.Core.Shared/           ← Enums, constants, config POCOs
├── ETR.ToDo.Core.EF/               ← DbContext, migrations, EF configs
├── ETR.ToDo.Services.Core/         ← Service interfaces, DTOs
├── ETR.ToDo.Services/              ← Service implementations, Mapperly mappers
├── ETR.ToDo.Web.Core/              ← Base controller, JWT, middleware
├── ETR.ToDo.Web.Host/              ← Entry point, Program.cs, controllers
├── ETR.ToDo.Tests/                 ← xUnit unit tests
├── ETR.ToDo.Tests.Api.BDD/         ← Reqnroll BDD integration tests
├── ETR.ToDo.Tests.Playwright/      ← Playwright E2E (TypeScript, Vitest runner)
├── ETR.ToDo.Frontend/              ← Angular 21 SPA
└── BlueTech.All.sln
```

---

## Runtime Stack

| Component | Technology |
|---|---|
| Backend runtime | .NET 10 |
| Frontend framework | Angular 21 |
| Database | SQLite (dev) — file: `FHToDo.db` |
| ORM | EF Core 10 |
| Auth | JWT (60 min) + refresh tokens (7 days) |
| Logging | Serilog — rolling file + console |

---

## Local Development

### Prerequisites
- .NET 10 SDK
- Node.js 20+ / npm
- Angular CLI 21: `npm install -g @angular/cli`
- (Optional) EF Core tools: `dotnet tool install -g dotnet-ef`

### Backend
```bash
# From repo root
dotnet restore
dotnet build

# Run the API (port 5000 by default)
cd ETR.ToDo.Web.Host
dotnet run
```

Database is created automatically on startup via:
```csharp
// Program.cs
app.Services.MigrateDatabase();  // applies pending EF migrations
app.Services.SeedDatabase();     // seeds system users
```

### Frontend
```bash
cd ETR.ToDo.Frontend
npm install
ng serve   # http://localhost:4200
```

### Running All Tests
```bash
# Backend unit tests
dotnet test ETR.ToDo.Tests/

# BDD integration tests
dotnet test ETR.ToDo.Tests.Api.BDD/

# Playwright E2E (requires API + frontend running)
cd ETR.ToDo.Tests.Playwright
npx playwright install chromium
npx playwright test

# Frontend unit tests
cd ETR.ToDo.Frontend
npm test
```

---

## Database Management

### SQLite Files (gitignored)
```
*.db
*.db-shm
*.db-wal
```
Never commit SQLite files. Connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=../FHToDo.db"
}
```

### EF Migrations
```bash
# Add migration (run from repo root)
dotnet ef migrations add <MigrationName> \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host

# Apply migrations manually
dotnet ef database update \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host
```
Migrations are applied automatically on startup — never require manual `database update` in any environment.

---

## Configuration by Environment

### `appsettings.json` (committed, safe defaults)
```json
{
  "ConnectionStrings": { "DefaultConnection": "Data Source=../FHToDo.db" },
  "Jwt": {
    "Secret": "REPLACE_IN_PRODUCTION",
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

### `appsettings.Production.json` (never committed)
Override `Jwt:Secret`, `ConnectionStrings:DefaultConnection`, and `Cors:Origins` for production.

Environment variables override any file setting:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=...
Jwt__Secret=...
```

---

## Logging & Observability

### Serilog Configuration
```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```
Log files: `ETR.ToDo.Web.Host/logs/log-YYYY-MM-DD.txt`

### API Request Logging
Every HTTP request/response is written to the `ApiLogs` table via `ApiLoggingMiddleware`. Accessible at `GET /api/logs` (Admin + Dev roles only).

### Health Check
```
GET /health
```
Returns 200 if database is reachable. Frontend `app.initializer.ts` blocks bootstrap on a failed health check and shows a non-dismissable dialog.

---

## CORS Policy

Configured in `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)   // from appsettings Cors:Origins
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
app.UseCors("CorsPolicy");
```

For production, set `Cors:Origins` to the exact frontend origin — never use `AllowAnyOrigin()`.

---

## Build Artefacts

### Backend publish
```bash
dotnet publish ETR.ToDo.Web.Host \
  -c Release \
  -o ./publish/api \
  --self-contained false
```

### Frontend build
```bash
cd ETR.ToDo.Frontend
ng build --configuration production
# Output: dist/fh-to-do-frontend/browser/
```
Set `outputPath` in `angular.json` if serving from API's `wwwroot`.

---

## Security Checklist (Before Production)

- [ ] Replace `Jwt:Secret` with a 256-bit random value
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Lock `Cors:Origins` to exact frontend URL
- [ ] Confirm SQLite file is outside the web root
- [ ] Verify `appsettings.Production.json` is in `.gitignore`
- [ ] Run `dotnet publish -c Release` — confirm no debug symbols
- [ ] Confirm Serilog log path is writable by the app process user
- [ ] Rotate system user passwords from seeded defaults
