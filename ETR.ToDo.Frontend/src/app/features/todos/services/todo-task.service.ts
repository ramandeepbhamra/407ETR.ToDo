import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { TodoTask } from '../models/todo-task.model';
import { TodoSubTask } from '../models/todo-sub-task.model';
import { TodoTaskCreateRequest } from '../models/todo-task-create-request.model';
import { TodoTaskUpdateRequest } from '../models/todo-task-update-request.model';
import { TodoSubTaskCreateRequest } from '../models/todo-sub-task-create-request.model';
import { UpdateTaskOrderDto } from '../models/update-task-order.model';

/**
 * HTTP service for all task and subtask operations.
 * Subtask routes are nested under their parent task: `/api/tasks/{taskId}/subtasks`.
 * Toggle endpoints use PATCH; all others use standard REST verbs.
 */
@Injectable({ providedIn: 'root' })
export class TodoTaskService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/tasks`;

  /** Returns all tasks in the given list, sorted by their `order` field on the server. */
  getByList(listId: string): Observable<TodoTask[]> {
    return this.http
      .get<ApiResponse<TodoTask[]>>(`${this.base}?listId=${listId}`)
      .pipe(map(r => r.data!));
  }

  /** Returns all tasks marked as favourite by the current user, across all lists. */
  getFavourites(): Observable<TodoTask[]> {
    return this.http
      .get<ApiResponse<TodoTask[]>>(`${this.base}/favourites`)
      .pipe(map(r => r.data!));
  }

  /** Creates a new task in the specified list. */
  create(request: TodoTaskCreateRequest): Observable<TodoTask> {
    return this.http
      .post<ApiResponse<TodoTask>>(this.base, request)
      .pipe(map(r => r.data!));
  }

  /** Updates an existing task's title, list assignment, and due date. */
  update(id: string, request: TodoTaskUpdateRequest): Observable<TodoTask> {
    return this.http
      .put<ApiResponse<TodoTask>>(`${this.base}/${id}`, request)
      .pipe(map(r => r.data!));
  }

  /** Soft-deletes a task and all its subtasks. */
  delete(id: string): Observable<void> {
    return this.http
      .delete<ApiResponse<void>>(`${this.base}/${id}`)
      .pipe(map(() => undefined));
  }

  /** Toggles the `isCompleted` flag on a task. Returns the updated task. */
  toggleComplete(id: string): Observable<TodoTask> {
    return this.http
      .patch<ApiResponse<TodoTask>>(`${this.base}/${id}/complete`, {})
      .pipe(map(r => r.data!));
  }

  /** Toggles the `isFavourite` flag on a task. Returns the updated task. */
  toggleFavourite(id: string): Observable<TodoTask> {
    return this.http
      .patch<ApiResponse<TodoTask>>(`${this.base}/${id}/favourite`, {})
      .pipe(map(r => r.data!));
  }

  /** Adds a new subtask to the specified parent task. */
  addSubTask(taskId: string, request: TodoSubTaskCreateRequest): Observable<TodoSubTask> {
    return this.http
      .post<ApiResponse<TodoSubTask>>(`${this.base}/${taskId}/subtasks`, request)
      .pipe(map(r => r.data!));
  }

  /** Toggles the `isCompleted` flag on a subtask. Returns the updated subtask. */
  toggleSubTaskComplete(taskId: string, subTaskId: string): Observable<TodoSubTask> {
    return this.http
      .patch<ApiResponse<TodoSubTask>>(`${this.base}/${taskId}/subtasks/${subTaskId}/complete`, {})
      .pipe(map(r => r.data!));
  }

  /** Updates an existing subtask's title. */
  updateSubTask(taskId: string, subTaskId: string, request: TodoSubTaskCreateRequest): Observable<TodoSubTask> {
    return this.http
      .put<ApiResponse<TodoSubTask>>(`${this.base}/${taskId}/subtasks/${subTaskId}`, request)
      .pipe(map(r => r.data!));
  }

  /** Soft-deletes a subtask. */
  deleteSubTask(taskId: string, subTaskId: string): Observable<void> {
    return this.http
      .delete<ApiResponse<void>>(`${this.base}/${taskId}/subtasks/${subTaskId}`)
      .pipe(map(() => undefined));
  }

  /**
   * Bulk-updates the display order of tasks within a list after a drag-drop reorder.
   * Sends the full ordered array of `{ id, order }` pairs for the affected list.
   */
  updateOrder(listId: string, updates: UpdateTaskOrderDto[]): Observable<void> {
    return this.http
      .put<ApiResponse<void>>(`${this.base}/update-order`, { listId, updates })
      .pipe(map(() => undefined));
  }
}
