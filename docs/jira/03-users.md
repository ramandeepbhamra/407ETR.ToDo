# Epic: User Management

**Summary:** Admin-only feature. Admins can list all users with pagination/sort, create new users, edit existing users, deactivate users, and change user roles. System users cannot be deleted or have their role changed.

---

## Story 3.1 — List and Search Users

**As an** Admin  
**I want to** view a paginated, sortable list of all users  
**So that** I can manage the user base

### Task 3.1.1 — Backend: List Users Endpoint

**[DEV-BE]** Create `User` entity in `ETR.ToDo.Core/Entities/`
- Extends `BaseEntity<Guid>`
- Fields: `Name` (string, max 100), `Email` (string, max 256), `PasswordHash` (string), `Role` (UserRole enum), `IsSystemUser` (bool), `IsActive` (bool)

**[DEV-BE]** Create `UserConfiguration` in `ETR.ToDo.Core.EF/Configurations/`
- Unique index on `Email`
- Soft-delete query filter

**[DEV-BE]** Add `DbSet<User>` to `ToDoDbContext`

**[DEV-BE]** Create EF migration `AddUserTable`

**[DEV-BE]** Create DTOs: `UserDto`, `GetUsersInputDto` (extends `PagedAndSortedRequestDto`)

**[DEV-BE]** Create `IUserService` interface + `UserService`
- `GetAllAsync(GetUsersInputDto)` → `PagedResultDto<UserDto>`
- Filter, sort, paginate via LINQ on `_repo.GetAll()`

**[DEV-BE]** Create `UsersController` — `[Authorize(Roles = "Admin")]` at class level
- `GET /api/users` — paged + sorted

**[QA-UNIT]** Unit test `UserService.GetAllAsync`
- Returns paged result
- Sort by name ascending/descending
- Excludes soft-deleted users

**[QA-BDD]** BDD feature: `Users.Management.feature`
- Scenario: Admin lists users → 200 with pagination
- Scenario: Basic user lists users → 403

---

## Story 3.2 — Create User

**As an** Admin  
**I want to** create a new user account  
**So that** I can onboard team members without them self-registering

### Task 3.2.1 — Backend: Create User Endpoint

**[DEV-BE]** Create `CreateUserDto` — name, email, password, role
- Duplicate email check → `InvalidOperationException`
- BCrypt hash password on create

**[DEV-BE]** `UserService.CreateAsync()` — enforce unique email, hash password

**[DEV-BE]** `POST /api/users` — Admin only

**[QA-UNIT]** Unit test `UserService.CreateAsync`
- Happy path: user created with hashed password
- Duplicate email: throws `InvalidOperationException`

**[QA-BDD]**
- Scenario: Admin creates user → 201
- Scenario: duplicate email → 400

### Task 3.2.2 — Frontend: Create User Dialog

**[DEV-FE]** Create `UserCreateDialogComponent` (lazy-loaded)
- Name, email, password, role selector fields
- Error UX pattern on all fields
- On success: user list refreshes

**[DEV-FE]** Create `user.service.ts` in `features/users/services/`

**[QA-FE]** Vitest: `user.service.spec.ts`

**[QA-E2E]** Playwright: Admin creates user → appears in list

---

## Story 3.3 — Edit User

**As an** Admin  
**I want to** edit a user's name, email, and role  
**So that** I can keep user records accurate

### Task 3.3.1 — Backend: Update User Endpoint

**[DEV-BE]** Create `UpdateUserDto` — name, email, role

**[DEV-BE]** `UserService.UpdateAsync()` — cannot change role of `IsSystemUser`, ownership/existence check

**[DEV-BE]** `PUT /api/users/{id}` — Admin only

**[QA-UNIT]** Unit test `UserService.UpdateAsync`
- Happy path: user updated
- System user role change: throws `InvalidOperationException`
- Not found: throws `KeyNotFoundException`

**[QA-BDD]**
- Scenario: Admin edits user → 200
- Scenario: change system user role → 400

### Task 3.3.2 — Frontend: Edit User Dialog

**[DEV-FE]** Create `UserEditDialogComponent` (lazy-loaded)
- Pre-populated with current user data
- Role selector disabled for system users
- Error UX pattern

**[QA-E2E]** Playwright: Admin edits user name → list reflects change

---

## Story 3.4 — Deactivate / Reactivate User

**As an** Admin  
**I want to** deactivate a user  
**So that** they cannot log in without permanently deleting their data

### Task 3.4.1 — Backend: Deactivate Endpoint

**[DEV-BE]** `PATCH /api/users/{id}/deactivate` and `PATCH /api/users/{id}/activate`
- `UserService.DeactivateAsync()` — sets `IsActive = false`; system users cannot be deactivated
- Login check: `IsActive = false` → throw `UnauthorizedAccessException`

**[QA-UNIT]** Unit test
- Deactivate: success
- Deactivate system user: throws `InvalidOperationException`
- Login while inactive: throws `UnauthorizedAccessException`

**[QA-BDD]**
- Scenario: deactivate user → 200
- Scenario: inactive user logs in → 401

### Task 3.4.2 — Frontend: Deactivate UI

**[DEV-FE]** Deactivate/reactivate action in user list row (confirm dialog before action)

**[QA-E2E]** Playwright: Admin deactivates user → user shown as inactive in list
