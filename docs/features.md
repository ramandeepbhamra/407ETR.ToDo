# Features — ETR.ToDo

A complete reference for every feature in the application: what each role can do, the rules enforced server-side, and how the UI surfaces those rules.

---

## Roles

The application has four roles. Role is set at account creation and can be changed by an Admin (except on system users — see [User Management](#user-management)).

| Role | Description |
|---|---|
| **Basic** | Standard user. Can manage their own tasks and lists, subject to plan limits. |
| **Premium** | Elevated user. Same as Basic with no plan limits. |
| **Admin** | Administrator. Unlimited tasks + user management + API log access. |
| **Dev** | Developer. Unlimited tasks + DevTools access + API log access. |

### Feature Access Matrix

| Feature | Basic | Premium | Admin | Dev |
|---|---|---|---|---|
| Dashboard (public) | ✅ | ✅ | ✅ | ✅ |
| Task Lists | ✅ (max 10) | ✅ | ✅ | ✅ |
| Todo Tasks | ✅ (max 10/list) | ✅ | ✅ | ✅ |
| Subtasks | ✅ | ✅ | ✅ | ✅ |
| Favourites | ✅ | ✅ | ✅ | ✅ |
| User Management | ❌ | ❌ | ✅ | ❌ |
| API Logs | ❌ | ❌ | ✅ | ✅ |
| DevTools | ❌ | ❌ | ❌ | ✅ |

> **Limits are enforced server-side.** The frontend never makes count decisions — the API returns a 400 with an upgrade message when a limit is hit, and the frontend shows an upgrade prompt dialog.

---

## Dashboard

The dashboard is the public landing page — visible to all visitors including unauthenticated users.

**What it contains:**
- Hero section with a call-to-action (login or register)
- Features section — overview of what the app offers
- Testimonials, pricing, and video sections
- Footer

**Authenticated users** see personalised CTAs (e.g. "Go to Tasks"). The dashboard is always the root `/` route and is never replaced by a redirect on login.

---

## Authentication & Authorisation

### How Login Works

Authentication is **dialog-only** — there is no `/auth/login` page. Login and registration are Angular Material dialogs that can be opened from the navigation bar or any upgrade prompt. This means the user never loses their place in the app when authenticating.

**Login flow:**
1. User enters email and password
2. API validates credentials with BCrypt
3. On success: API returns a JWT access token (60 min) + a refresh token (7 days)
4. Frontend stores both tokens in `localStorage` and sets the current user signal
5. A default "My Tasks" list is created automatically for newly registered Basic/Premium users

### JWT Access Token

- Valid for **60 minutes**
- Contains claims: user ID (`sub`), email, role, expiry (`exp`)
- Attached to every API request as `Authorization: Bearer {token}` by the auth interceptor

### Refresh Token

- Valid for **7 days**
- Stored in `localStorage`
- **Rotated on every use** — consuming a refresh token always produces a new one and revokes the old one
- On a 401 response from any non-auth endpoint, the interceptor automatically attempts a token refresh and retries the original request — the user never sees a login prompt mid-session unless the refresh token itself has expired

### Session Management

- **Idle timeout:** 15 minutes of inactivity
- **Warning countdown:** 30 seconds before auto-logout, a non-dismissable countdown dialog appears
- If the user takes no action, they are logged out and the login dialog opens
- Timeout values are configured in `appsettings.json` and delivered to the frontend via `/api/config` — never hardcoded in the frontend

### Backend Authorisation

Controllers are protected with `[Authorize]` and `[Authorize(Roles = "...")]` attributes. All business logic additionally enforces ownership — a user can only read or mutate their own data. Violations throw:

| Exception | HTTP Response |
|---|---|
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 403 Forbidden |
| `InvalidOperationException` | 400 Bad Request |

### Frontend Guards

| Guard | Protects |
|---|---|
| `authGuard` | All authenticated routes — redirects to `/` and opens login dialog |
| `adminGuard` | `/users` — Admin role only |
| `devUserGuard` | `/dev-tools` — Dev role only |
| `adminOrDevUserGuard` | `/logs` — Admin or Dev role |

---

## Task Lists

Task lists are the top-level containers for tasks. Each list is owned by a single user and displayed in the left sidebar of the Todos section.

### Rules by Role

| Action | Basic | Premium | Admin | Dev |
|---|---|---|---|---|
| Create | ✅ (max 10) | ✅ | ✅ | ✅ |
| Rename | ✅ (own lists only) | ✅ | ✅ | ✅ |
| Delete | ✅ (own lists only) | ✅ | ✅ | ✅ |

**Basic limit:** When a Basic user has 10 active lists and tries to create another, the API returns a 400 and the frontend shows the upgrade prompt dialog. The limit is configurable in `appsettings.json` (`BasicUserTaskListLimit`).

### Behaviour

- **Create:** Opens a dialog with a title field (1–100 characters). On success, the list appears in the sidebar immediately.
- **Rename:** Opens the same dialog pre-filled with the current title.
- **Delete:** Requires confirmation via a dialog. Deleting a list is a **cascading soft-delete** — all tasks and subtasks inside the list are also soft-deleted in a single database transaction. If the user is currently viewing the deleted list, they are redirected to the Favourites view.
- **Default list:** When a new Basic or Premium user registers, a "My Tasks" list is created automatically.

---

## Todo Tasks

Tasks live inside a task list. Each task is owned by the user who created it.

### Rules by Role

| Action | Basic | Premium | Admin | Dev |
|---|---|---|---|---|
| Create | ✅ (max 10/list) | ✅ | ✅ | ✅ |
| Edit | ✅ (own tasks only) | ✅ | ✅ | ✅ |
| Delete | ✅ (own tasks only) | ✅ | ✅ | ✅ |
| Complete/Incomplete | ✅ | ✅ | ✅ | ✅ |
| Favourite | ✅ | ✅ | ✅ | ✅ |
| Reorder (drag-drop) | ✅ | ✅ | ✅ | ✅ |

**Basic limit:** When a Basic user has 10 tasks in a list and tries to create another, the API returns a 400 and the upgrade prompt dialog is shown. The limit is configurable (`BasicUserTaskLimit`).

### Task Fields

| Field | Description |
|---|---|
| Title | Required. 1–255 characters. |
| Due Date | Optional. Date only (no time). Displayed as `MM/DD/YYYY`. |
| Is Completed | Toggleable. |
| Is Favourite | Toggleable. |
| Order | Integer. Controls display order within the list. |

### Completion

- Toggling a task **complete** also marks all its subtasks as complete (cascade).
- Toggling a task back to **incomplete** does **not** revert subtask completion — subtask states are preserved as-is.
- Completed tasks are displayed in a separate "Completed" tab within the list.
- Active (incomplete) tasks are shown in the default "Active" tab, sorted by their `Order` field.

### Due Date

- Optional — tasks without a due date display no date indicator.
- When a due date is in the past and the task is not completed, it is shown with an **overdue** visual indicator (red highlight).
- Overdue state is calculated client-side at render time using today's date.

### Favourite

- Any task can be starred/unstarred regardless of completion state.
- Favourited tasks appear in the cross-list **Favourites** view (accessible from the sidebar).
- Un-favouriting a task in the Favourites view removes it from that view immediately without a page reload.

### Edit

- Inline editing — double-click (desktop) or tap (mobile) the task title to enter edit mode.
- Title and due date can be changed. The list the task belongs to can also be changed.
- Validation: title cannot be empty or exceed 255 characters.

### Delete

- Requires confirmation via a dialog.
- The confirmation message includes the number of subtasks that will also be deleted.
- Soft-delete — the task and all its subtasks are flagged `IsDeleted = true` in the database.

### Reorder

- Active tasks can be reordered via drag-and-drop within the list.
- Dropping a task updates `Order` values optimistically in the UI, then persists the new order to the backend in a single bulk call.
- Completed tasks are not reorderable.

---

## Subtasks

Subtasks are checklist items inside a task. They have a title and a completion state only — no due date, no favourite, no order.

### Rules by Role

All roles follow the same rules for subtasks — there is no per-role limit on subtask count.

| Action | All Roles |
|---|---|
| Add subtask | ✅ (task must not be completed) |
| Edit subtask title | ✅ (own tasks only) |
| Toggle subtask complete | ✅ |
| Delete subtask | ✅ (own tasks only) |

### Behaviour

- **Add:** Cannot add a subtask to a completed task. The task must be marked incomplete first.
- **Edit:** Inline — click the edit icon to enter edit mode. Title must be 1–255 characters.
- **Complete:** Toggling a subtask's completion state is independent of the parent task — completing all subtasks does not automatically complete the parent task.
- **Delete:** Requires confirmation via a dialog. Soft-deleted.

---

## Favourites

The Favourites view is a cross-list view showing all tasks the current user has starred, regardless of which list they belong to.

- Accessible from the sidebar (star icon) and the bottom navigation bar on mobile.
- Tasks are rendered using the same `TodoItemComponent` as the list view — all actions (edit, complete, delete, subtask management) work identically.
- Un-favouriting a task removes it from the Favourites view immediately.
- Completing or deleting a task from the Favourites view updates/removes it in real time.

---

## DevTools

DevTools is a **Dev role only** section. It provides a live component browser for inspecting and testing the application's UI primitives.

**What it contains:**
- Buttons — all button variants (primary, secondary, icon, etc.)
- Inputs — form field variations
- Progress — spinners and progress bars
- Dialog — dialog trigger examples
- Panels — expansion panel variants
- Tables — data table with sorting
- Stepper — multi-step form pattern
- Tabs — tab navigation examples

The DevTools layout mirrors the Todos layout — it has the same sidenav structure with a component list on the left and the selected component rendered on the right. Responsive behaviour (overlay sidenav on mobile, side sidenav on desktop) is identical.

### Theming

The theme selector is available to all authenticated users via the navigation bar. It is not restricted to any role.

**How it works:**
- 4 built-in preset themes: Teal (default), Blue, Rose, Dark Teal
- Each theme defines 6 CSS custom properties applied to `document.body`: `--primary`, `--primary-light`, `--primary-dark`, `--background`, `--error`, `--ripple`
- All colours in the app — components, form fields, buttons, icons — read from these properties
- No hex values are hardcoded anywhere in SCSS or templates
- Theme preference is **in-memory only** — it resets to the default (Teal) on page refresh

---

## API Logs

API Logs is restricted to **Admin and Dev** roles. It provides a searchable, paginated view of all HTTP requests and responses logged by Serilog to the `ApiLogs` database table.

### What is logged

Every API request is captured by the Serilog pipeline including: timestamp, HTTP method, path, status code, response time, and the authenticated user's name (if any).

### Filters available

- Date range (from / to)
- Log level
- Keyword search (matches path and message)
- Pagination and sorting

### Rules by Role

| Action | Basic | Premium | Admin | Dev |
|---|---|---|---|---|
| View API Logs | ❌ | ❌ | ✅ | ✅ |

---

## User Management

User Management is restricted to **Admin** role only. It provides a paginated table of all user accounts with create, edit, and deactivate capabilities.

### Rules by Role

| Action | Basic | Premium | Admin | Dev |
|---|---|---|---|---|
| View user list | ❌ | ❌ | ✅ | ❌ |
| Create user | ❌ | ❌ | ✅ | ❌ |
| Edit user | ❌ | ❌ | ✅ | ❌ |
| Deactivate user | ❌ | ❌ | ✅ | ❌ |

> The Admin viewing the list does not see themselves — their own account is excluded from the results.

### User Fields

| Field | Rules |
|---|---|
| First Name | Required. 1–100 characters. |
| Last Name | Required. 1–100 characters. |
| Email | Required. Valid email format. Must be unique. Max 256 characters. |
| Password | Required on create. 8–100 characters. Not shown on edit. |
| Phone Number | Optional. Max 20 characters. |
| Role | Required. Basic, Premium, Admin, or Dev. |
| Is Active | Toggle. Inactive users cannot log in. |

### System Users vs Normal Users

| | System User | Normal User |
|---|---|---|
| Created by | Database seeding (on first run) | Admin via UI, or self-registration |
| Role changeable | ❌ — role field is locked in the edit dialog | ✅ |
| Can be deactivated | ✅ | ✅ |
| Can be edited (name, email, phone) | ✅ | ✅ |

System users are the seed accounts created automatically when the application starts for the first time. The first two accounts in each role are flagged `IsSystemUser = true`. Their role cannot be changed by an Admin — the role selector is disabled in the edit dialog — to protect the integrity of the known test accounts.

### Filters available

- Name (first or last)
- Email
- Role
- Active / Inactive status
- System user / Normal user
- Pagination and sorting (by any column)

### Self-registration

Any visitor can register a new **Basic** account via the registration dialog. Self-registered users are always Basic — role assignment requires an Admin.
