# Epic: API Logs

**Summary:** Admin and Dev users can view all logged API requests and responses. Supports filtering, sorting, and pagination. Logs are written to the `ApiLogs` DB table by `ApiLoggingMiddleware` on every request.

---

## Story 4.1 — API Request Logging (Infrastructure)

**As a** system  
**I want** every API request and response to be logged to the database  
**So that** Admins and Dev users can audit and debug API activity

### Task 4.1.1 — Backend: ApiLoggingMiddleware

**[DEV-BE]** Create `ApiLog` entity in `ETR.ToDo.Core/Entities/`
- Extends `BaseEntity<Guid>`
- Fields: `Method` (string), `Path` (string), `StatusCode` (int), `RequestBody` (string?), `ResponseBody` (string?), `DurationMs` (long), `UserId` (Guid?)

**[DEV-BE]** Create `ApiLogConfiguration` — index on `CreatedDate`, index on `UserId`

**[DEV-BE]** Add `DbSet<ApiLog>` to `ToDoDbContext`

**[DEV-BE]** Create EF migration `AddApiLogTable`

**[DEV-BE]** Create `ApiLoggingMiddleware` in `ETR.ToDo.Web.Core/`
- Captures method, path, status code, request/response bodies, duration
- Writes to `ApiLogs` table via scoped `ToDoDbContext`
- Never logs `/health` endpoint

**[QA-UNIT]** Unit test middleware captures correct fields and skips `/health`

**[QA-BDD]**
- Scenario: any authenticated request → log entry created
- Scenario: `/health` → no log entry

---

## Story 4.2 — View API Logs

**As an** Admin or Dev user  
**I want to** view, filter, sort, and paginate API logs  
**So that** I can monitor and debug API activity

### Task 4.2.1 — Backend: Logs Endpoint

**[DEV-BE]** Create DTOs: `ApiLogDto`, `GetApiLogsInputDto` (extends `PagedAndSortedRequestDto`, adds `method?`, `path?`, `statusCode?`, `from?`, `to?` filters)

**[DEV-BE]** Create `IApiLogService` interface + `ApiLogService`
- `GetAllAsync(GetApiLogsInputDto)` → `PagedResultDto<ApiLogDto>`
- `.WhereIf()` for optional filters

**[DEV-BE]** Create `ApiLogsController` — `[Authorize(Roles = "Admin,Dev")]`
- `GET /api/logs` — filtered, sorted, paged

**[QA-UNIT]** Unit test `ApiLogService`
- Filter by method: only matching logs returned
- Filter by date range
- Pagination returns correct page

**[QA-BDD]**
- Scenario: Admin views logs → 200 with entries
- Scenario: Basic user views logs → 403
- Scenario: filter by status code 400 → only 400s returned

### Task 4.2.2 — Frontend: API Logs View

**[DEV-FE]** Create `ApiLogsListComponent` in `features/api-logs/`
- Table with columns: method, path, status, duration, date
- Pagination via Angular Material paginator
- Filter controls: method dropdown, status code, date range
- Colour coding: 2xx success, 4xx warn, 5xx error — using CSS custom properties only

**[DEV-FE]** Create `api-log.service.ts` in `features/api-logs/services/`

**[DEV-FE]** Wire route `logs` behind `adminOrDevUserGuard`

**[QA-FE]** Vitest: `api-log.service.spec.ts`
- `getAll()` maps query params correctly

**[QA-E2E]** Playwright: `api-logs.spec.ts`
- Admin navigates to Logs → table visible with entries
- Filter by method GET → only GET rows shown
- Basic user cannot access logs route (guard redirects)
