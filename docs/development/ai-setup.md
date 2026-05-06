# AI Assistant Setup — ETR.ToDo

This project is pre-configured for AI-assisted development with both **Claude Code** and **GitHub Copilot**. Every AI tool in this project has been given full project context — architecture, coding conventions, forbidden patterns, and feature checklists — so you do not need to manually prompt for project standards. They are enforced automatically.

---

## Overview

| Tool | Config Location | How It Works |
|---|---|---|
| Claude Code | `CLAUDE.md` (repo root) | Read automatically at the start of every Claude Code session |
| GitHub Copilot | `.github/copilot-instructions.md` | Picked up automatically by Copilot in VS Code and Rider |
| AI Agent Definitions | `.ai/agents/` | Specialist agent prompts for each domain |
| AI Context Files | `.ai/context/` | Deep project context referenced by agents |

---

## Claude Code (`CLAUDE.md`)

`CLAUDE.md` at the repo root is Claude Code's project instruction file. It is loaded automatically at the start of every session — you never need to paste conventions into the chat.

**What it covers:**
- Collaboration rules (ask before making changes, one type per file, no cleanup beyond the task)
- Full tech stack reference
- Critical backend rules (Clean Architecture, Mapperly, soft delete, exception semantics, no try/catch in services)
- Critical frontend rules (signals, `inject()`, `@if`/`@for`, responsive, theming, lazy dialogs)
- Complete forbidden patterns table
- Validation field length constants
- New feature checklist for both backend and frontend

**How to use:**
Simply open the project in Claude Code and start working. Claude will follow all project conventions without being reminded.

---

## GitHub Copilot (`.github/copilot-instructions.md`)

The `.github/copilot-instructions.md` file is automatically read by GitHub Copilot in VS Code and JetBrains Rider. It contains the same core conventions as `CLAUDE.md` in Copilot's expected format.

**How to use:**
Open any file in the project with Copilot enabled — suggestions will already follow project conventions (Mapperly not AutoMapper, `input()`/`output()` not `@Input()`/`@Output()`, etc.).

---

## AI Agent Definitions (`.ai/agents/`)

The `.ai/agents/` folder contains specialist agent definitions. Each file is a detailed system prompt scoped to a specific domain. Use these when asking an AI assistant to work on a specific area of the codebase — paste the relevant agent file as the system prompt, or reference it in your session.

| Agent | File | Domain |
|---|---|---|
| Backend Architect | `backend-architect.md` | API design, Clean Architecture, entities, services, EF Core, auth, security |
| Frontend Architect | `frontend-architect.md` | Angular 21 signals, routing, state, theming, responsive, dialogs, session |
| QA / Test Engineer | `qa-engineer.md` | xUnit unit tests, Reqnroll BDD, Playwright E2E, Vitest frontend tests |
| DevOps Engineer | `devops-engineer.md` | Build, migrations, configuration, CORS, Serilog, health checks, production checklist |

### What Each Agent Knows

**Backend Architect** — authoritative on:
- 7-layer Clean Architecture boundaries and dependency rules
- `ApiResponse<T>` envelope, route conventions, controller thinness
- Entity design (`BaseEntity<Guid>`, soft delete, `DateOnly`, audit fields)
- Service patterns, exception semantics, `ExceptionHandlingMiddleware`
- JWT + refresh token rotation architecture
- Mapperly mapper patterns
- EF Core Fluent API, query filters, pagination

**Frontend Architect** — authoritative on:
- Angular 21 standalone components, lazy loading, guard hierarchy
- Signal patterns (`signal()`, `computed()`, `effect()`, `model()`)
- `ResponsiveService` (CDK `BreakpointObserver`) — never CSS `@media`
- Dialog-only auth (no `/auth/login` route)
- `authInterceptor` — token attachment and 401 refresh queue
- Theming via CSS custom properties (`--primary`, `--background`, etc.)
- Session management (`IdleService`, `SessionWarningDialogComponent`)
- Error UX pattern (snackbar + shake + Material warn colour)

**QA / Test Engineer** — authoritative on:
- xUnit + Moq + FluentAssertions + MockQueryable patterns
- Reqnroll BDD — Gherkin feature file structure, step definitions, `WebApplicationFactory`
- Playwright — selector strategy, auth fixture, `waitForLoadState` conventions
- Vitest — signal testing, validator tests, co-located spec files
- Coverage targets and what NOT to test (framework internals, generated code)

**DevOps Engineer** — authoritative on:
- EF Core migration commands
- Environment configuration (`appsettings.json` vs `appsettings.Production.json`)
- Serilog setup and log file paths
- CORS configuration and production lockdown
- Build and publish commands for both API and Angular
- Pre-production security checklist

---

## AI Context Files (`.ai/context/`)

The `.ai/context/` folder contains deep project context documents used as reference material by AI agents. Unlike the agent definitions (which define behaviour), these describe the project as it currently stands.

| File | Contents |
|---|---|
| `project-overview.md` | What the app does, roles, feature areas, tech stack summary |
| `architecture.md` | Layer diagram, entity design, routing, state management, auth flow, responsive layout, theming |
| `coding-standards.md` | Naming conventions, entity rules, DTO rules, Mapperly patterns, signal patterns, error UX, git conventions |

---

## Recommended Workflow

When starting a new feature or asking an AI for help with a specific area:

1. **Claude Code** — just start the session. `CLAUDE.md` is auto-loaded.
2. **For a specific domain** — reference the relevant `.ai/agents/` file to give the AI full specialist context.
3. **For architecture questions** — reference `.ai/context/architecture.md`.
4. **For convention questions** — reference `.ai/context/coding-standards.md`.
5. **Copilot** — just write code. `.github/copilot-instructions.md` is auto-applied.
