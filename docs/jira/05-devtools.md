# Epic: DevTools

**Summary:** Dev-role-only feature. Provides a component browser with live props for inspecting UI components. Layout mirrors the Todos sidenav layout. Accessible only to users with the Dev role.

---

## Story 5.1 — DevTools Component Browser

**As a** Dev user  
**I want to** browse UI components with live, editable props  
**So that** I can inspect and test component behaviour without writing code

### Task 5.1.1 — Backend: Dev Role Guard (already in auth epic, reference only)

No additional backend work — DevTools is frontend-only. Route is guarded by `devUserGuard`.

### Task 5.1.2 — Frontend: DevTools Layout + Component Browser

**[DEV-FE]** Create `DevtoolsComponent` in `features/devtools/`
- Sidenav layout mirroring `TodoLayoutComponent`
- Uses same `SidenavService` for open/close state
- No `@media` — uses `ResponsiveService` signals

**[DEV-FE]** Create `devtools.routes.ts`
- Route `dev-tools` behind `devUserGuard`
- Lazy-loaded

**[DEV-FE]** Create component browser panel
- Left sidenav: list of registered components
- Main area: selected component rendered with live props
- Props editor: inputs rendered as form controls (text, boolean, number)

**[DEV-FE]** Register at least these components in the browser:
- `ConfirmDialogComponent`
- `AppNavigationComponent`
- `AppBottomNavComponent`
- `AppThemeSelectorComponent`

**[QA-E2E]** Playwright: `devtools.spec.ts`
- Dev user navigates to `/dev-tools` → component browser visible
- Selects a component → renders in main area
- Admin user cannot access `/dev-tools` → redirected
- Basic user cannot access `/dev-tools` → redirected
