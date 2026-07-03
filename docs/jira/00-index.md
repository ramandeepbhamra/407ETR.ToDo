# ETR.ToDo — Jira Backlog Index

## Structure

Each file represents one Epic. Stories, Tasks, and Sub-tasks are nested within.

## Sub-task Track Legend

- **[DEV-BE]** — Backend development (.NET / EF Core / API)
- **[DEV-FE]** — Frontend development (Angular / signals / UI)
- **[QA-UNIT]** — Backend xUnit unit tests
- **[QA-BDD]** — Reqnroll BDD integration tests
- **[QA-E2E]** — Playwright end-to-end tests
- **[QA-FE]** — Vitest frontend unit tests
- **[QA-XRAY-CREATE]** — Create manual test cases in Xray
- **[QA-XRAY-EXEC]** — Execute manual test cases in Xray

> **Note:** Xray test case documentation is maintained separately in [docs/xray/](../xray/)  
> Each story links to its corresponding Xray test documentation.

## Epics

| File | Epic | Description |
|---|---|---|
| [01-auth.md](01-auth.md) | Authentication | Login, register, JWT, refresh tokens, idle session |
| [02-todos.md](02-todos.md) | Todo Management | Task lists, tasks, subtasks, due dates, favourites |
| [03-users.md](03-users.md) | User Management | Admin-only: list, create, edit, deactivate, role change |
| [04-api-logs.md](04-api-logs.md) | API Logs | Admin+Dev: view, filter, sort, paginate request logs |
| [05-devtools.md](05-devtools.md) | DevTools | Dev-only: component browser with live props |
| [06-dashboard.md](06-dashboard.md) | Dashboard | Public landing page, login CTA overlay |
| [07-infrastructure.md](07-infrastructure.md) | Infrastructure | Health check, config API, theming, responsive, session |
