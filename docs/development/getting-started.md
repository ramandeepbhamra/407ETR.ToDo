# Getting Started

## Prerequisites

| Tool | Version | Install |
|---|---|---|
| .NET SDK | 10.x | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Node.js | 20.x LTS | [nodejs.org](https://nodejs.org/) |
| Angular CLI | 21.x | `npm install -g @angular/cli` |
| EF Core CLI | latest | `dotnet tool install -g dotnet-ef` |
| Visual Studio | 2022 17.x+ | Community edition supported |

---

## Backend Setup

### 1. Configure Development Settings (First Time Only)

The project uses `appsettings.Development.json` for local configuration. This file is **gitignored** to protect secrets.

**Setup steps:**
```bash
cd ETR.ToDo.Web.Host
cp appsettings.Development.json.example appsettings.Development.json
```

Then edit `appsettings.Development.json` and add your credentials:
- **Jira:** Get API token from https://id.atlassian.com/manage-profile/security/api-tokens
- **Xray:** Get Client ID/Secret from Xray Cloud API settings

> **Note:** If you don't need Jira/Xray integration, you can leave the placeholder values as-is. They're only required if you're working with test management features.

For detailed setup instructions, see [docs/xray/setup-guide.md](../xray/setup-guide.md).

### 2. Choose a Launch Profile

| Profile | HTTP URL | HTTPS URL | Recommendation |
|---|---|---|---|
| `http` (Kestrel) | `http://localhost:5214` | — | ✅ Use this |
| `https` (Kestrel) | `http://localhost:5214` | `https://localhost:7182` | — |
| IIS Express | `http://localhost:52333` | `https://localhost:44387` | VS default |

In Visual Studio, select the profile from the dropdown next to the ▶ button. **Use the Kestrel `http` profile** — it matches the default `apiBaseUrl` in the frontend and the ports used in Playwright E2E tests.

To trust the dev certificate (HTTPS profiles only):
```bash
dotnet dev-certs https --trust
```

### 3. Run the API

```bash
dotnet run --project ETR.ToDo.Web.Host
```

On first run, the app automatically:
- Creates the SQLite database file
- Applies all EF Core migrations via `MigrateAsync()`
- Seeds 40 test users (10 per role: Basic, Admin, Dev)

### 4. Verify

- Swagger UI: `http://localhost:5214/swagger`
- Scalar UI: `http://localhost:5214/scalar/v1`
- Health check: `http://localhost:5214/health`

### 5. Test Credentials

Seed accounts for all four roles (Basic, Premium, Admin, Dev) are created automatically on first run. See the seeding logic in the project, or register a new Basic account directly via the app.

### Database Location

| How you run | DB file location |
|---|---|
| `dotnet run` from solution root | `FHToDo.db` (solution root) |
| Visual Studio F5 | `ETR.ToDo.Web.Host/FHToDo.db` |

Both work — the connection string `Data Source=../FHToDo.db` resolves relative to the startup project.

### Manual Migration (Fallback Only)

If auto-migration fails:

```bash
dotnet ef database update \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host
```

### Reset the Database

Stop the app, delete the DB file, then restart — it is recreated automatically:

```bash
# From solution root
rm FHToDo.db
dotnet run --project ETR.ToDo.Web.Host
```

---

## Frontend Setup

### 1. Install Dependencies

```bash
cd ETR.ToDo.Frontend
npm install
```

### 2. Set Backend URL

Open `src/environments/environment.ts` and confirm `apiBaseUrl` matches the active backend profile:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5214',
};
```

| Active profile | `apiBaseUrl` |
|---|---|
| Kestrel `http` (recommended) | `http://localhost:5214` |
| Kestrel `https` | `https://localhost:7182` |
| IIS Express HTTP | `http://localhost:52333` |

### 3. Start Dev Server

```bash
ng serve
# → http://localhost:4200
```

---

## CORS

The backend allows requests from these origins (configured in `appsettings.json`):

```
http://localhost:3000
http://localhost:4200
```

`http://localhost:4200` is included by default — no proxy configuration required.

---

## Running Tests

### Backend Unit Tests

```bash
dotnet test ETR.ToDo.Tests
dotnet test ETR.ToDo.Tests --verbosity detailed
dotnet watch test --project ETR.ToDo.Tests
```

### BDD Integration Tests

```bash
dotnet test ETR.ToDo.Tests.Api.BDD
dotnet test ETR.ToDo.Tests.Api.BDD --filter "DisplayName~Authentication"
```

### E2E Tests (Playwright)

Requires both frontend and backend running:

```bash
cd ETR.ToDo.Tests.Playwright
npm install
npx playwright install chromium
npm test

# Interactive UI mode
npm run test:ui

# Debug mode
npm run test:debug
```

### Frontend Unit Tests

```bash
cd ETR.ToDo.Frontend
npm test                   # watch mode
npm run test:coverage      # with coverage report
```

---

## Common Issues

**`ERR_CONNECTION_REFUSED`**
Backend is not running or `environment.ts` points to the wrong port. Verify the API is healthy via the Swagger URL.

**CORS preflight blocked**
Ensure `UseCors` appears before `UseHttpsRedirection` in `Program.cs`:
```csharp
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
```

**`No DbContext was found` (EF CLI)**
Install EF Core tools:
```bash
dotnet tool install -g dotnet-ef
```

**Database file is locked**
Multiple app instances running simultaneously. Stop all instances and delete lock files:
```bash
rm FHToDo.db-shm FHToDo.db-wal
```

**Playwright: "Navigation timeout"**
Angular frontend is not running at `http://localhost:4200`. Start it with `ng serve` first.

**Playwright: "Chromium not installed"**
```bash
npx playwright install chromium
```
