# Epic: Todo Management

**Summary:** Core feature — authenticated users manage task lists, tasks, subtasks, due dates, favourites, and completion. Basic users are limited to 10 lists and 10 tasks per list (configurable). All deletes are soft deletes. Inline edit on double-click (desktop) / tap (mobile).

---

## Story 2.1 — Task List Management

**As an** authenticated user  
**I want to** create, rename, and delete task lists  
**So that** I can organise my tasks by context

### Task 2.1.1 — Backend: Task List CRUD

**[DEV-BE]** Create `TaskList` entity in `ETR.ToDo.Core/Entities/`
- Extends `BaseEntity<Guid>`
- Fields: `Name` (string), `UserId` (Guid FK)

**[DEV-BE]** Create `TaskListConfiguration` in `ETR.ToDo.Core.EF/Configurations/`
- Max length 100 on `Name`
- Index on `UserId`
- Soft-delete query filter

**[DEV-BE]** Add `DbSet<TaskList>` to `ToDoDbContext`

**[DEV-BE]** Create EF migration `AddTaskListTable`

**[DEV-BE]** Create DTOs in `ETR.ToDo.Services.Core/TaskLists/Dto/`
- `TaskListDto`, `CreateTaskListDto`, `UpdateTaskListDto`

**[DEV-BE]** Create `ITaskListService` interface

**[DEV-BE]** Create `TaskListMapper` (Mapperly) in `ETR.ToDo.Services/Mapping/`

**[DEV-BE]** Create `TaskListService` — enforce `BasicUserTaskListLimit` (10), soft delete, ownership check

**[DEV-BE]** Create `TaskListsController` — `GET`, `POST`, `PUT /{id}`, `DELETE /{id}`

**[QA-UNIT]** Unit test `TaskListService`
- Create: below limit → success
- Create: at limit (Basic) → throws `InvalidOperationException`
- Delete: not owner → throws `UnauthorizedAccessException`
- Delete: not found → throws `KeyNotFoundException`
- Rename: happy path

**[QA-BDD]** BDD feature: `TaskLists.Create.feature`, `TaskLists.Limits.feature`
- Scenario: create list → 201
- Scenario: create 11th list (Basic) → 400
- Scenario: rename list → 200
- Scenario: delete list → 200 (soft delete)
- Scenario: other user cannot delete my list → 403

### Task 2.1.2 — Frontend: Task List Sidenav

**[DEV-FE]** Create `TodoSidenavComponent`
- Lists rendered via `@for`
- Active list highlighted
- `SidenavService.isOpen` drives open/close
- Mobile: overlay mode, desktop: side mode (no `@media`)

**[DEV-FE]** Create `TaskListDialogComponent` (lazy-loaded) — create + rename in same dialog

**[DEV-FE]** Create `ConfirmDialogComponent` in `shared/components/` — reusable delete confirmation

**[DEV-FE]** Create `todo-task-list.service.ts` in `features/todos/services/`
- `getAll()`, `create()`, `update()`, `delete()` — return `Observable<T>`

**[QA-FE]** Vitest: `todo-task-list.service.spec.ts`
- `getAll()` maps response correctly
- `create()` sends correct payload

**[QA-E2E]** Playwright: `task-list.spec.ts`
- Create list → appears in sidenav
- Rename list → sidenav updates
- Delete list → confirm dialog, list removed

---

## Story 2.2 — Task Creation and Management

**As an** authenticated user  
**I want to** create, edit, complete, and delete tasks within a list  
**So that** I can track my to-dos

### Task 2.2.1 — Backend: Task CRUD

**[DEV-BE]** Create `TodoTask` entity in `ETR.ToDo.Core/Entities/`
- Extends `BaseEntity<Guid>`
- Fields: `Title` (string), `DueDate` (DateOnly?), `IsCompleted` (bool), `IsFavourite` (bool), `ListId` (Guid FK), `UserId` (Guid FK)

**[DEV-BE]** Create `TodoTaskConfiguration` in `ETR.ToDo.Core.EF/Configurations/`
- Max length 255 on `Title`
- Index on `ListId`, `UserId`
- Soft-delete query filter

**[DEV-BE]** Add `DbSet<TodoTask>` to `ToDoDbContext`

**[DEV-BE]** Create EF migration `AddTodoTaskTable`

**[DEV-BE]** Create DTOs: `TodoTaskDto`, `CreateTodoTaskDto`, `UpdateTodoTaskDto`, `GetTasksInputDto`

**[DEV-BE]** Create `ITodoTaskService` interface

**[DEV-BE]** Create `TaskMapper` (Mapperly)

**[DEV-BE]** Create `TodoTaskService`
- Enforce `BasicUserTaskLimit` (10 per list)
- Ownership check on update/delete
- `DateOnly` for `DueDate` — never `DateTime`

**[DEV-BE]** Create `TodoTasksController`
- `GET /api/tasks?listId=` — filter by list
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `PATCH /api/tasks/{id}/complete` — toggle completion
- `PATCH /api/tasks/{id}/favourite` — toggle favourite
- `DELETE /api/tasks/{id}` — soft delete

**[QA-UNIT]** Unit test `TodoTaskService`
- Create: below limit → success
- Create: at limit (Basic) → throws `InvalidOperationException`
- Complete: not owner → throws `UnauthorizedAccessException`
- Update: not found → throws `KeyNotFoundException`
- DueDate stored as `DateOnly`

**[QA-BDD]** BDD feature: `TodoTasks.Create.feature`, `TodoTasks.Complete.feature`
- Scenario: create task with title + due date → 201
- Scenario: create 11th task → 400
- Scenario: complete task → 200, isCompleted = true
- Scenario: favourite task → 200, isFavourite = true
- Scenario: delete task → 200 (soft)

### Task 2.2.2 — Frontend: Task List View + Inline Edit

**[DEV-FE]** Create `TodoLayoutComponent` — shell with sidenav + task view area

**[DEV-FE]** Create `TodoTaskListComponent` — renders list of tasks via `@for`

**[DEV-FE]** Create `TodoTaskItemComponent`
- Completion toggle (checkbox)
- Favourite toggle (star icon)
- Double-click / tap → inline edit mode
- Due date display with overdue styling via CSS custom properties

**[DEV-FE]** Create `TodoTaskFormComponent` — add task form (title + due date)
- Error UX pattern: shake + warn + snackbar
- Max 255 chars on title

**[DEV-FE]** Create `todo-task.service.ts` in `features/todos/services/`

**[DEV-FE]** Wire upgrade prompt: when limit reached, lazy-load `UpgradePromptDialogComponent`

**[QA-FE]** Vitest: `todo-task.service.spec.ts`

**[QA-E2E]** Playwright: `create-task.spec.ts`, `complete-task.spec.ts`
- Create task → appears in list
- Complete task → checkbox checked, title styled
- Favourite → star filled
- Double-click task → inline edit, save → updated title shown
- 11th task → upgrade dialog shown

---

## Story 2.3 — Subtask Management

**As an** authenticated user  
**I want to** add subtasks under a task  
**So that** I can break work into smaller steps

### Task 2.3.1 — Backend: Subtask CRUD

**[DEV-BE]** Create `SubTask` entity
- Extends `BaseEntity<Guid>`
- Fields: `Title` (string, max 255), `IsCompleted` (bool), `TodoTaskId` (Guid FK)

**[DEV-BE]** Create `SubTaskConfiguration` — index on `TodoTaskId`, soft-delete filter

**[DEV-BE]** Add `DbSet<SubTask>` to `ToDoDbContext`

**[DEV-BE]** Create EF migration `AddSubTaskTable`

**[DEV-BE]** Create DTOs: `SubTaskDto`, `CreateSubTaskDto`, `UpdateSubTaskDto`

**[DEV-BE]** Create `ISubTaskService`, `SubTaskMapper`, `SubTaskService`

**[DEV-BE]** Create `SubTasksController`
- `GET /api/tasks/{taskId}/subtasks`
- `POST /api/tasks/{taskId}/subtasks`
- `PATCH /api/tasks/{taskId}/subtasks/{id}/complete`
- `DELETE /api/tasks/{taskId}/subtasks/{id}`

**[QA-UNIT]** Unit test `SubTaskService`
- Create: parent task not owned → throws `UnauthorizedAccessException`
- Complete subtask: happy path
- Delete: soft delete verified

**[QA-BDD]** BDD feature: `SubTasks.feature`
- Scenario: add subtask → 201
- Scenario: complete subtask → 200
- Scenario: delete subtask → 200 (soft)

### Task 2.3.2 — Frontend: Subtask UI

**[DEV-FE]** Create `TodoSubtaskListComponent` — expandable under parent task
- `@for` over subtasks
- Inline add subtask field
- Completion toggle per subtask
- Error UX on blank subtask title

**[DEV-FE]** Create `sub-task.service.ts`

**[QA-FE]** Vitest: `sub-task.service.spec.ts`

**[QA-E2E]** Playwright: subtask creation + completion flows

---

## Story 2.4 — Mobile Responsive Todo Layout

**As a** mobile user  
**I want** the todo UI to work on small screens  
**So that** I can manage tasks on my phone

### Task 2.4.1 — Frontend: Mobile Layout

**[DEV-FE]** `TodoLayoutComponent` — sidenav overlay mode on `smallWidth()`
- Hamburger button in toolbar (shown only when `smallWidth() && isAuthenticated()`)
- `SidenavService.toggle()` on hamburger click
- No `@media` queries — use `ResponsiveService` signals only

**[DEV-FE]** Create `AppBottomNavComponent` in `shared/components/`
- Fixed bottom bar
- Role-aware links (Todos always visible; Users for Admin; Logs for Admin+Dev)
- Shown only when `smallWidth() && isAuthenticated()`

**[QA-E2E]** Playwright: mobile viewport tests
- Sidenav closed by default on mobile
- Hamburger opens sidenav as overlay
- Bottom nav visible on mobile, hidden on desktop
