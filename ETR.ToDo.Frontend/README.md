# ETR.ToDo.Frontend — Angular 21 SPA

Angular 21 standalone signals-based SPA for the ETR.ToDo task management application.

---

## Quick Start

```bash
npm install
ng serve          # → http://localhost:4200
npm test          # unit tests (watch)
npm run test:coverage
npm run build     # production build
```

Requires backend running at `http://localhost:5214`. See [Getting Started](../docs/development/getting-started.md).

---

## Project Structure

```
src/app/
├── core/
│   ├── guards/           authGuard, adminGuard, devUserGuard, adminOrDevUserGuard
│   ├── interceptors/     authInterceptor (JWT attach + 401 refresh)
│   ├── services/         AuthService, ResponsiveService, SidenavService,
│   │                     ThemingService, ThemeSelectorService, IdleService, ConfigService
│   ├── initializers/     appInitializer (health check + config load before bootstrap)
│   ├── directives/       TrimOnBlurDirective
│   ├── validators/       noWhitespace, passwordMatch
│   ├── enums/            UserRole
│   └── utils/            date.util.ts
├── features/
│   ├── auth/             login + register dialogs (lazy-loaded)
│   ├── todos/            TodoLayout, TodoSidebar, TodoItem, TodoForm, dialogs
│   ├── users/            user management (Admin only)
│   ├── api-logs/         log viewer (Admin + Dev)
│   ├── devtools/         component browser (Dev only)
│   ├── dashboard/        public landing page
│   └── profile/          user profile dialog
├── layout/               AppLayoutComponent (shell)
└── shared/
    ├── components/       AppNavigationComponent, AppBottomNavComponent,
    │                     ConfirmDialogComponent, AppThemeSelectorComponent,
    │                     SessionWarningDialogComponent, HealthCheckDialogComponent
    ├── models/           ApiResponse<T>
    └── services/         UpgradeDialogService
```

---

## Key Conventions

| Rule | Detail |
|---|---|
| DI | `inject()` — never constructor injection |
| Inputs/Outputs | `input()` / `output()` — never `@Input()` / `@Output()` |
| Control flow | `@if` / `@for` — never `*ngIf` / `*ngFor` |
| HTTP | Services return `Observable<T>`; components subscribe and write to signals |
| Dialogs | Always lazy-loaded via dynamic `import()` |
| Auth | Dialog-only — no `/auth/login` route |
| Responsive | `ResponsiveService` signals only — never CSS `@media` queries |
| Colours | `var(--primary)` etc — never hardcoded hex values |
| Error UX | Snackbar + `[color]="'warn'"` + `[class.shake]` on every form field |

---

## Testing

**Framework:** Vitest — co-located `*.spec.ts` files beside each source file.

```bash
npm test                   # watch mode
npm run test:coverage      # coverage report
npm run test:ui            # Vitest UI
```

| Coverage target | Goal |
|---|---|
| Services | 80%+ |
| Guards | 100% |
| Validators | 100% |
| Components | 70%+ |

See [qa-engineer agent](../.ai/agents/qa-engineer.md) for test patterns.

---

## Configuration Files

| File | Purpose |
|---|---|
| `angular.json` | Angular CLI config |
| `tsconfig.json` | TypeScript config |
| `vitest.config.ts` | Test runner config |
| `src/test-setup.ts` | Angular test environment setup |
| `src/environments/` | `apiBaseUrl` per environment |
| `tailwind.config.js` | Tailwind (layout/spacing only) |
