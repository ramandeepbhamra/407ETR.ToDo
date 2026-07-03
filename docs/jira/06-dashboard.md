# Epic: Dashboard

**Jira Epic:** ETR-5  
**Summary:** Public landing page visible to unauthenticated users. Shows a login CTA overlay. Authenticated users see their name and a link to their tasks. No route-based auth — the login dialog opens from the CTA.

---

## Story 6.1 — Public Landing Page

**Jira Story:** ETR-6  
**Test Documentation:** [docs/xray/ETR-6-public-landing-page.md](../xray/ETR-6-public-landing-page.md)

**As an** unauthenticated visitor
**I want to** see a landing page with a clear call to action
**So that** I understand what the app does and can sign in

### Task 6.1.1 — Frontend: Dashboard Home Component

**Jira Task:** ETR-7

**[DEV-FE]** Create `DashboardHomeComponent` at route `/`
- Public — no guard
- Rendered inside `AppLayoutComponent` (toolbar + router-outlet)
- Login CTA: button that calls `AuthDialogService.openLogin()`
- Register CTA: button that calls `AuthDialogService.openRegister()`
- When `isAuthenticated()` — show welcome message + "Go to My Tasks" link to `/todos`

**[DEV-FE]** Create `AppLayoutComponent` in `layout/`
- Shell wrapping all routes
- Toolbar with: app title/logo, nav links (hidden on mobile), sign-in button (unauthenticated), user menu (authenticated), theme selector
- `AppNavigationComponent` for desktop nav links (role-aware)
- `AppBottomNavComponent` shown on mobile when authenticated

**[DEV-FE]** Create `AppNavigationComponent` in `shared/components/`
- Desktop nav links: Todos (authGuard), Users (adminGuard), Logs (adminOrDevGuard), DevTools (devGuard)
- Shown only when `!responsiveService.smallWidth()`

**[DEV-FE]** Create `AppThemeSelectorComponent` in `shared/components/`
- Theme toggle panel — opens/closes via `ThemeSelectorService`
- Applies CSS custom properties via `ThemingService`
- Preference persisted to `localStorage`

**[DEV-UNIT]** Vitest: `dashboard-home.component.spec.ts`
- Unauthenticated: login + register CTA buttons rendered
- Authenticated: welcome message and "Go to My Tasks" link rendered
- `AppThemeSelectorComponent`: theme applied writes correct CSS vars

**[QA-BDD-API]** Not applicable — Dashboard is frontend-only, no API endpoint

**[QA-E2E]** Playwright Cucumber: `dashboard.feature`
- Scenario: unauthenticated user visits `/` — landing page visible with sign-in button
- Scenario: sign-in button clicked — login dialog opens, URL does not change
- Scenario: authenticated user visits `/` — sees welcome message and "Go to My Tasks" link
- Scenario: theme selector toggled — colours change without page reload

**[QA-XRAY-CREATE]** Create manual test cases in Xray for:
**Jira Subtask:** ETR-14 ← Transition to **Done** (7 test cases created)  
**Test Cases:** ETR-16, ETR-17, ETR-19, ETR-20, ETR-21, ETR-26, ETR-27  
**Preconditions:** ETR-28, ETR-29, ETR-30, ETR-31  
**Test Set:** ETR-32 — "ETR-6 Landing Page Tests"  
**Test Plan:** ETR-33 — "Sprint 1 - Dashboard Tests"  
**Documentation:** [docs/xray/ETR-6-public-landing-page.md](../xray/ETR-6-public-landing-page.md)

Test Coverage:
- Landing page visible to unauthenticated users (ETR-16)
- Login CTA opens dialog, URL does not change (ETR-17)
- Register CTA opens dialog (ETR-26)
- Authenticated welcome message and task link (ETR-27)
- Theme selector persists preference after page reload (ETR-19)
- Nav links visible on desktop, hidden on mobile (ETR-20)
- Bottom nav visible on mobile when authenticated (ETR-21)

**[QA-XRAY-EXEC]** Execute all Dashboard manual test cases in Xray and log results
**Jira Subtask:** ETR-15 ← Transition to **Done** after execution
