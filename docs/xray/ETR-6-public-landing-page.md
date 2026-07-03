# Test Cases — ETR-6: Public Landing Page

**Epic:** ETR-5 — Dashboard  
**Story:** ETR-6 — 6.1 Public Landing Page ← Parent requirement for all test cases  
**Xray Create Subtask:** ETR-14 — [QA-XRAY-CREATE] Manual test cases in Xray  
**Xray Execute Subtask:** ETR-15 — [QA-XRAY-EXEC] Execute manual test cases in Xray

> **Note:** All test cases are linked to **ETR-6** via "Tests" relationship in Jira.  
> Test cases are created as type **Manual** with step-by-step actions and expected results.  
> Gherkin/Cucumber syntax is used only in `.feature` files in `ETR.ToDo.Tests.Playwright`.

---

## Xray Artifacts

| Artifact | Key | Description |
|---|---|---|
| Test Set | ETR-32 | ETR-6 Landing Page Tests — groups all 7 test cases |
| Test Plan | ETR-33 | Sprint 1 - Dashboard Tests |
| Preconditions | ETR-28, ETR-29, ETR-30, ETR-31 | Shared setup requirements |

### Preconditions

| Key | Name | Description |
|---|---|---|
| ETR-28 | User is not authenticated | Browser has no auth token, user is logged out |
| ETR-29 | User is logged in as Basic user | Valid Basic user is authenticated with active session |
| ETR-30 | Desktop viewport | Browser viewport is set to desktop size (width > 1000px) |
| ETR-31 | Mobile viewport | Browser viewport is set to mobile size (width ≤ 600px) |

---

## TC_ETR_6_001 — Landing page loads for unauthenticated user

**Tags:** `@ETR-6 @ETR-14 @TC_ETR_6_001 @smoke @regression`  
**Xray Test Issue:** ETR-16  
**Suite:** smoke, regression  
**Preconditions:** ETR-28 (User is not authenticated)

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | Open the browser without logging in | No auth token present |
| 2 | Navigate to `/` | Dashboard home page loads successfully |
| 3 | Inspect the page content | "Sign In" button is visible |
| 4 | Inspect the page content | "Register" button is visible |

### Feature File Scenario (Playwright — created separately)

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_001 @smoke @regression
Scenario: Unauthenticated user sees the public landing page
  Given I am not logged in
  When I navigate to "/"
  Then the dashboard home page is visible
  And a "Sign In" button is visible
  And a "Register" button is visible
```

---

## TC_ETR_6_002 — Login CTA opens dialog, URL does not change

**Tags:** `@ETR-6 @ETR-14 @TC_ETR_6_002 @smoke @regression`  
**Xray Test Issue:** ETR-17  
**Suite:** smoke, regression  
**Preconditions:** ETR-28 (User is not authenticated)

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | Open the browser without logging in | No auth token present |
| 2 | Navigate to `/` | Dashboard home page loads |
| 3 | Click the "Sign In" button | Login dialog appears on screen |
| 4 | Check the browser URL | URL remains `/` — no route change |

### Feature File Scenario (Playwright — created separately)

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_002 @smoke @regression
Scenario: Sign In button opens login dialog without route change
  Given I am not logged in
  And I am on "/"
  When I click the "Sign In" button
  Then the login dialog is visible
  And the URL is still "/"
```

---

## TC_ETR_6_003 — Register CTA opens register dialog

**Tags:** `@ETR-6 @ETR-14 @TC_ETR_6_003 @regression`  
**Xray Test Issue:** ETR-26  
**Suite:** regression  
**Preconditions:** ETR-28 (User is not authenticated)

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | Open the browser without logging in | No auth token present |
| 2 | Navigate to `/` | Dashboard home page loads |
| 3 | Click the "Register" button | Register dialog appears on screen |
| 4 | Check the browser URL | URL remains `/` — no route change |

### Feature File Scenario (Playwright — created separately)

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_003 @regression
Scenario: Register button opens register dialog without route change
  Given I am not logged in
  And I am on "/"
  When I click the "Register" button
  Then the register dialog is visible
  And the URL is still "/"
```

---

## TC_ETR_6_004 — Authenticated user sees welcome message and task link

**Tags:** `@ETR-6 @ETR-14 @TC_ETR_6_004 @smoke @regression`  
**Xray Test Issue:** ETR-27  
**Suite:** smoke, regression  
**Preconditions:** ETR-29 (User is logged in as Basic user)

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | Log in as a Basic user | Auth token stored, user is authenticated |
| 2 | Navigate to `/` | Dashboard home page loads |
| 3 | Inspect the page content | Welcome message containing the user's name is visible |
| 4 | Inspect the page content | "Go to My Tasks" link is visible |
| 5 | Inspect the page content | "Sign In" button is not visible |

### Feature File Scenario (Playwright — created separately)

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_004 @smoke @regression
Scenario: Authenticated user sees personalised welcome and Go to My Tasks link
  Given I am logged in as a Basic user
  When I navigate to "/"
  Then I see a welcome message containing my name
  And a "Go to My Tasks" link is visible
  And the "Sign In" button is not visible
```

---

## TC_ETR_6_005 — Theme selector persists preference after page reload

**Tags:** `@ETR-6 @ETR-14 @TC_ETR_6_005 @regression`  
**Xray Test Issue:** ETR-19  
**Suite:** regression  
**Preconditions:** None (works for both authenticated and unauthenticated users)

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | Navigate to `/` | Dashboard home page loads |
| 2 | Open the theme selector | Theme panel is visible |
| 3 | Select a theme other than the default | Theme is applied immediately — colours change |
| 4 | Reload the page | Page reloads |
| 5 | Inspect the applied theme | Previously selected theme is still active |

### Feature File Scenario (Playwright — created separately)

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_005 @regression
Scenario: Theme preference is persisted across page reloads
  Given I am on "/"
  When I open the theme selector
  And I select a theme other than the default
  And I reload the page
  Then the selected theme is still applied
```

---

## TC_ETR_6_006 — Desktop nav links are visible on large viewport

**Tags:** `@ETR-6 @ETR-14 @TC_ETR_6_006 @regression`  
**Xray Test Issue:** ETR-20  
**Suite:** regression  
**Preconditions:** ETR-29 (User is logged in), ETR-30 (Desktop viewport)

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | Log in as a Basic user | User is authenticated |
| 2 | Set viewport to desktop size (>1000px wide) | Viewport is desktop |
| 3 | Navigate to `/` | Dashboard home page loads |
| 4 | Inspect the toolbar | Top navigation links are visible |
| 5 | Inspect the bottom of the screen | Bottom navigation bar is not visible |

### Feature File Scenario (Playwright — created separately)

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_006 @regression
Scenario: Navigation links are visible on desktop viewport
  Given I am logged in as a Basic user
  And I am using a desktop viewport
  When I navigate to "/"
  Then the top navigation bar is visible
  And the bottom navigation bar is not visible
```

---

## TC_ETR_6_007 — Bottom nav visible on mobile when authenticated

**Tags:** `@ETR-6 @ETR-14 @TC_ETR_6_007 @regression`  
**Xray Test Issue:** ETR-21  
**Suite:** regression  
**Preconditions:** ETR-29 (User is logged in), ETR-31 (Mobile viewport)

### Xray Steps (plain English)

| # | Action | Expected Result |
|---|---|---|
| 1 | Log in as a Basic user | User is authenticated |
| 2 | Set viewport to mobile size (≤600px wide) | Viewport is mobile |
| 3 | Navigate to `/` | Dashboard home page loads |
| 4 | Inspect the bottom of the screen | Bottom navigation bar is visible |
| 5 | Inspect the toolbar | Top navigation links are not visible |

### Feature File Scenario (Playwright — created separately)

```gherkin
@ETR-6 @ETR-14 @TC_ETR_6_007 @regression
Scenario: Bottom navigation bar is visible on mobile when authenticated
  Given I am logged in as a Basic user
  And I am using a mobile viewport
  When I navigate to "/"
  Then the bottom navigation bar is visible
  And the top navigation links are not visible
```

---

## Summary

| Test Case | Description | Suite | Preconditions |
|---|---|---|
| TC_ETR_6_001 | Landing page loads for unauthenticated user | smoke, regression | ETR-28 |
| TC_ETR_6_002 | Login CTA opens dialog, URL unchanged | smoke, regression | ETR-28 |
| TC_ETR_6_003 | Register CTA opens dialog, URL unchanged | regression | ETR-28 |
| TC_ETR_6_004 | Authenticated user sees welcome + task link | smoke, regression | ETR-29 |
| TC_ETR_6_005 | Theme selector persists after reload | regression | None |
| TC_ETR_6_006 | Desktop nav visible on large viewport | regression | ETR-29, ETR-30 |
| TC_ETR_6_007 | Bottom nav visible on mobile when authenticated | regression | ETR-29, ETR-31 |

---

## Xray Structure

```
ETR-5 (Epic: Dashboard)
└── ETR-6 (Story: Public Landing Page) ← Parent requirement
    ├── ETR-14 (Subtask: Create test cases) ← Transition to Done after creation
    ├── ETR-15 (Subtask: Execute test cases)
    ├── ETR-32 (Test Set: ETR-6 Landing Page Tests)
    │   ├── ETR-16 (Test: TC_ETR_6_001) → Precondition: ETR-28
    │   ├── ETR-17 (Test: TC_ETR_6_002) → Precondition: ETR-28
    │   ├── ETR-26 (Test: TC_ETR_6_003) → Precondition: ETR-28
    │   ├── ETR-27 (Test: TC_ETR_6_004) → Precondition: ETR-29
    │   ├── ETR-19 (Test: TC_ETR_6_005)
    │   ├── ETR-20 (Test: TC_ETR_6_006) → Preconditions: ETR-29, ETR-30
    │   └── ETR-21 (Test: TC_ETR_6_007) → Preconditions: ETR-29, ETR-31
    └── ETR-33 (Test Plan: Sprint 1 - Dashboard Tests)
        └── Contains: ETR-32 (Test Set)
```
