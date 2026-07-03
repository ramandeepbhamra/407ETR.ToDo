# Epic: Infrastructure & Cross-Cutting Concerns

**Summary:** Foundational work — health check, database seeding, EF migrations, Serilog logging, CORS, theming system, responsive service, session management, exception handling middleware, and CI build pipeline.

---

## Story 7.1 — API Health Check

**As the** system  
**I want** a health endpoint  
**So that** the frontend can detect API downtime before bootstrapping

### Task 7.1.1 — Backend

**[DEV-BE]** Register `AddHealthChecks().AddDbContextCheck<ToDoDbContext>("database")` in `Program.cs`

**[DEV-BE]** Map `GET /health` — returns 200 if DB reachable, 503 otherwise

**[QA-BDD]**
- Scenario: API running → `/health` returns 200
- Scenario: DB unreachable → `/health` returns 503

### Task 7.1.2 — Frontend

**[DEV-FE]** `appInitializer` calls `GET /health` before bootstrap
- On failure: open `HealthCheckDialogComponent` with `disableClose: true`
- On success: proceed to config load

**[DEV-FE]** Create `HealthCheckDialogComponent` in `shared/components/` — non-dismissable, shows support email from `ConfigService`

**[QA-FE]** Vitest: `app.initializer.spec.ts`
- Health check failure opens health dialog
- Health check success proceeds to config load

---

## Story 7.2 — Database Seeding

**As a** developer  
**I want** the database seeded with system users on startup  
**So that** the app is immediately usable without manual setup

### Task 7.2.1 — Backend

**[DEV-BE]** Create `DatabaseSeeder` in `ETR.ToDo.Services/`
- Seed: 1 Admin system user, 1 Dev system user, sample Basic users for testing
- Idempotent — check before inserting
- BCrypt hash all passwords

**[DEV-BE]** Wire `app.Services.MigrateDatabase()` + `app.Services.SeedDatabase()` in `Program.cs`

**[QA-BDD]**
- Scenario: fresh database → seeded users exist and can log in

---

## Story 7.3 — Exception Handling Middleware

**As the** system  
**I want** all unhandled exceptions caught and returned as structured API responses  
**So that** clients always receive consistent error shapes

### Task 7.3.1 — Backend

**[DEV-BE]** Create `ExceptionHandlingMiddleware` in `ETR.ToDo.Web.Core/`
- `KeyNotFoundException` → 404 `ApiResponse`
- `UnauthorizedAccessException` → 403 `ApiResponse`
- `InvalidOperationException` → 400 `ApiResponse`
- All others → 500 `ApiResponse` (with ILogger, never Console.WriteLine)

**[QA-UNIT]** Unit test middleware maps each exception type to correct status code

**[QA-BDD]**
- Scenario: service throws `KeyNotFoundException` → response is 404 with `success: false`
- Scenario: service throws `InvalidOperationException` → response is 400

---

## Story 7.4 — Theming System

**As a** user  
**I want to** switch between colour themes  
**So that** I can personalise the app appearance

### Task 7.4.1 — Frontend

**[DEV-FE]** Create `ThemingService` in `core/services/`
- Writes CSS custom properties to `document.body`: `--primary`, `--primary-light`, `--primary-dark`, `--background`, `--error`, `--ripple`
- Persists selected theme to `localStorage`
- Never hardcodes hex values in SCSS

**[DEV-FE]** Create `ThemeSelectorService` in `core/services/`
- Manages open/close state of theme panel (signal-based)

**[DEV-FE]** Global `styles.scss`
- `@keyframes shake` + `.shake` class (used by error UX pattern across all forms)
- Material theme integration via `@use "@angular/material"`
- Never `@media` — all breakpoint logic in `ResponsiveService`

**[QA-FE]** Vitest: `theming.service.spec.ts`
- Theme applied writes correct CSS vars to body
- Preference persisted and read from localStorage

---

## Story 7.5 — Responsive Service

**As a** user on any device  
**I want** the layout to adapt to my screen size  
**So that** the app is usable on mobile, tablet, and desktop

### Task 7.5.1 — Frontend

**[DEV-FE]** Create `ResponsiveService` in `core/services/` (wraps CDK `BreakpointObserver`)
- `smallWidth = computed(...)` — true when ≤600px
- `mediumWidth = computed(...)` — true when 601–1000px
- `largeWidth = computed(...)` — true when >1000px

**[DEV-FE]** Create `SidenavService` in `core/services/`
- `isOpen = signal(false)`
- `effect()` auto-opens on tablet/desktop, auto-closes on mobile
- `toggle()` method

**[QA-FE]** Vitest: `responsive.service.spec.ts`
- `smallWidth()` true at 375px, false at 1024px

---

## Story 7.6 — CORS and Security Configuration

**As a** DevOps engineer  
**I want** CORS and JWT configured from `appsettings.json`  
**So that** production deployments are secure without code changes

### Task 7.6.1 — Backend

**[DEV-BE]** CORS policy in `Program.cs` — `WithOrigins()` from `appsettings Cors:Origins`; never `AllowAnyOrigin()`

**[DEV-BE]** JWT configured from `appsettings Jwt:*` — secret, issuer, audience, expiration

**[DEV-BE]** Production security checklist items (from devops-engineer.md):
- Replace `Jwt:Secret` with 256-bit random value
- Lock `Cors:Origins` to exact frontend URL
- `appsettings.Production.json` in `.gitignore`

**[QA-BDD]**
- Scenario: request from allowed origin → CORS headers present
- Scenario: request from disallowed origin → CORS headers absent

---

## Story 7.7 — Build and Test Pipeline

**As a** developer  
**I want** a reliable CI pipeline  
**So that** every commit is verified before merge

### Task 7.7.1 — DevOps

**[DEV-BE]** Verify `dotnet build` is clean (no warnings as errors)

**[DEV-BE]** Verify `dotnet test ETR.ToDo.Tests/` passes

**[DEV-BE]** Verify `dotnet test ETR.ToDo.Tests.Api.BDD/` passes

**[DEV-FE]** Verify `npm test` (Vitest) passes with coverage thresholds: Services ≥80%, Frontend services ≥70%

**[QA-E2E]** Playwright runs on Chromium with `retries: 2` on CI, `workers: 1` on CI

**[DEV-BE]** Confirm `*.db`, `*.db-shm`, `*.db-wal` are in `.gitignore`

**[DEV-BE]** Confirm `appsettings.Production.json` is in `.gitignore`
