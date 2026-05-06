# Scalability Roadmap

## Architectural Scaling

### 1. Migrate from SQLite to PostgreSQL or SQL Server
SQLite is single-writer — concurrent writes queue. EF Core and Fluent API configs are already database-agnostic, so this is a connection string + NuGet package swap with minimal code change. **Priority: high — hardest to retrofit with real data.**

### 2. Introduce CQRS in the Service Layer
Services currently mix reads and writes. Separating `IQueryHandler` (reads) from `ICommandHandler` (writes) via MediatR gives independent scaling, simpler caching on reads, and a clear audit trail on writes.

### 3. Output Caching for Read-Heavy Endpoints
`GET /api/config`, `GET /api/users`, and task list endpoints change infrequently. ASP.NET Core's built-in `AddOutputCache()` requires no architecture change and reduces DB load significantly.

### 4. Centralise Limit Enforcement in a Policy Service
`BasicUserTaskLimit` and `BasicUserTaskListLimit` are checked inline in service methods. A dedicated `IPlanPolicyService` returning limits by user/role makes future plan tiers (Pro, Enterprise) a single-file change.

### 5. Background Job Queue for Side Effects
Email notifications, audit events, and log archiving should run outside the HTTP request pipeline. Introduce Hangfire or .NET's `IHostedService` + `Channel<T>` for fire-and-forget jobs.

### 6. API Versioning
Add `Asp.Versioning` now before clients depend on current routes. Route-based versioning (`/api/v1/tasks`) is the most visible and cacheable approach. **Retrofitting after go-live is painful.**

### 7. Frontend Feature Flag Service
A lightweight `FeatureFlagService` backed by `ConfigService` (values from `/api/config`) gates UI features per role or environment without a deploy — enabling gradual rollouts.

### 8. Global Frontend Error Boundary + Structured Logging
Add a centralised Angular `ErrorHandler` to capture unhandled errors and send them to a logging endpoint or third-party (e.g. Sentry). The current per-component snackbar approach is insufficient for production diagnostics.

### 9. Multi-Device Refresh Token Support
Currently one refresh token per user — multi-device users invalidate each other's sessions on rotation. Add `DeviceId`/`UserAgent` to `RefreshToken` and allow multiple active tokens per user (one per device). **Hardest to retrofit after go-live.**

### 10. Granular Health Checks
`GET /health` currently checks only the DB. Add individual checks for disk space (logs), background job queue, and external services. Split into `/health/ready` (all dependencies) and `/health/live` (process alive) following Kubernetes conventions.

---

## Testing Scaling

### 1. Coverage Gates
Enforce minimum thresholds in `vitest.config.ts` (FE) and `coverlet` (BE). Without gates, coverage silently degrades. Target: 80% on services and components; 100% on guards and validators.

### 2. Contract Testing (Frontend ↔ Backend)
If a DTO property is renamed on the backend, the Angular service gets `undefined` silently. **Pact** consumer-driven contract testing generates a contract from Angular service calls and verifies it against the real API.

### 3. Mutation Testing
High coverage numbers are misleading if assertions are weak. **Stryker.NET** (BE) and **Stryker** (FE) mutate code and verify tests actually fail. Surfaces tests that assert nothing meaningful.

### 4. Performance Baselines
No tests currently verify response times. Add **k6** or **NBomber** scripts for critical paths (login, task list fetch, task create) and fail CI if p95 latency regresses beyond a threshold.

### 5. Role Boundary BDD Scenarios
Current Reqnroll scenarios focus on happy paths. The highest-value scenarios for this app are **authorization boundaries** — Basic user hitting task limit, Admin accessing another user's data, Basic user attempting admin endpoints.

### 6. Visual Regression Testing
Playwright's built-in screenshot comparison (`expect(page).toHaveScreenshot()`) catches UI regressions (theme changes, layout shifts) with baselines stored in the repo — free on top of the existing Playwright setup.

### 7. Test Data Builders
A `TodoTaskBuilder.WithTitle("x").WithUser(userId).Build()` pattern centralises test data construction. When an entity gains a new required field, you fix one builder instead of dozens of tests.

### 8. Isolated Database per Test Run
BDD tests share a `CustomWebApplicationFactory`. Parallel runs will collide. Use unique SQLite file names per test class and tear down after. This is the prerequisite for parallel BDD CI execution.

### 9. Post-Deploy Smoke Tests
Run a lightweight Playwright suite (5–10 tests) against the live environment immediately after deployment: health check, login flow, one authenticated action. Catches infrastructure issues tests can't catch locally.

### 10. Slow Test Threshold Enforcement
Add `--slow-test-threshold` to Vitest and a `[MaxTime]` convention for xUnit. Any test taking over 500ms is either hitting real I/O it shouldn't or is a parallelisation candidate.

---

## Feature Scaling Roadmap

### 1. Multilingual Support (i18n)
Enable global language support so users interact in their preferred language. Use Angular's `@angular/localize` and `ngx-translate` for runtime translation without rebuilds.

### 2. User-Configurable Locale Management
Allow users to select and persist culture/locale (country-based) to control language, formatting, and regional behaviour. Store preference server-side and sync to `ConfigService` on login.

### 3. RTL and LTR Layout Switching
Implement dynamic layout direction switching at runtime using Angular's `Dir` directive and CSS logical properties (`margin-inline-start` instead of `margin-left`) to support both Arabic/Hebrew and Latin scripts.

### 4. Localised Formatting (Date, Number, Currency)
All `DueDate` rendering, number formatting, and any future currency values must adapt to the user's locale automatically. Use Angular's built-in `DatePipe` and `DecimalPipe` with locale injection.

### 5. Translation-Ready UI
Replace all hardcoded strings in Angular templates with translation keys (`{{ 'TASK.ADD' | translate }}`). New languages added without any code changes — only new translation JSON files.

### 6. Cross-Culture Testing Coverage
Extend Playwright E2E tests to run scenarios in multiple locales (en-GB, ar-SA, de-DE) to prevent region-specific formatting bugs and verify RTL/LTR layout correctness.

### 7. Referral System
Allow users to invite others via unique referral links. Track successful registrations against a referral code stored on the `User` entity. Foundation for future reward mechanisms.

### 8. Email Integration via External Provider
Integrate Twilio SendGrid or similar to handle invitation emails, password resets, task reminders, and user lifecycle notifications. Use the background job queue (see Architectural #5) to send emails async.

### 9. Third-Party Authentication (OAuth)
Support login via Google and Microsoft using ASP.NET Core's `AddAuthentication().AddGoogle().AddMicrosoftAccount()`. No password required — external token mapped to existing user by email.

### 10. Two-Factor Authentication (2FA)
Add TOTP-based 2FA (Google Authenticator compatible) using `System.Security.Cryptography` for QR code generation. Enforce for Admin and Dev roles, optional for Basic. Store `TwoFactorEnabled` + `TwoFactorSecret` on `User`.
