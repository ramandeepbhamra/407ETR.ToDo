import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { TodoTaskList } from '../models/todo-task-list.model';
import { TodoTaskListCreateRequest } from '../models/todo-task-list-create-request.model';
import { TodoTaskListUpdateRequest } from '../models/todo-task-list-update-request.model';

@Injectable({ providedIn: 'root' })
export class TodoTaskListService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/tasklists`;

  getAll(): Observable<TodoTaskList[]> {
    return this.http
      .get<ApiResponse<TodoTaskList[]>>(this.base)
      .pipe(map(r => r.data!));
  }

  create(request: TodoTaskListCreateRequest): Observable<TodoTaskList> {
    return this.http
      .post<ApiResponse<TodoTaskList>>(this.base, request)
      .pipe(map(r => r.data!));
  }

  update(id: string, request: TodoTaskListUpdateRequest): Observable<TodoTaskList> {
    return this.http
      .put<ApiResponse<TodoTaskList>>(`${this.base}/${id}`, request)
      .pipe(map(r => r.data!));
  }

  delete(id: string): Observable<void> {
    return this.http
      .delete<ApiResponse<void>>(`${this.base}/${id}`)
      .pipe(map(() => undefined));
  }
}
