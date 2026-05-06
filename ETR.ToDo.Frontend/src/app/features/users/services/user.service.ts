import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { User } from '../models/user.model';
import { UserGetInput } from '../models/user-get-input.model';
import { UserCreateRequest } from '../models/user-create-request.model';
import { UserUpdateRequest } from '../models/user-update-request.model';
import { UserUpdateProfileRequest } from '../models/user-update-profile-request.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/users`;
  private readonly profileBase = `${environment.apiBaseUrl}/api/profile`;

  getUsers(input: UserGetInput): Observable<PagedResult<User>> {
    let params = new HttpParams()
      .set('page', input.page)
      .set('pageSize', input.pageSize)
      .set('sortDirection', input.sortDirection);

    if (input.sortBy)           params = params.set('sortBy', input.sortBy);
    if (input.email)            params = params.set('email', input.email);
    if (input.name)             params = params.set('name', input.name);
    if (input.role)             params = params.set('role', input.role);
    if (input.isActive != null)       params = params.set('isActive', input.isActive);
    if (input.isSystemUser != null)   params = params.set('isSystemUser', input.isSystemUser);

    return this.http
      .get<ApiResponse<PagedResult<User>>>(this.base, { params })
      .pipe(map(r => r.data!));
  }

  getById(id: string): Observable<User> {
    return this.http
      .get<ApiResponse<User>>(`${this.base}/${id}`)
      .pipe(map(r => r.data!));
  }

  create(request: UserCreateRequest): Observable<User> {
    return this.http
      .post<ApiResponse<User>>(this.base, request)
      .pipe(map(r => r.data!));
  }

  update(id: string, request: UserUpdateRequest): Observable<User> {
    return this.http
      .put<ApiResponse<User>>(`${this.base}/${id}`, request)
      .pipe(map(r => r.data!));
  }

  getProfile(): Observable<User> {
    return this.http
      .get<ApiResponse<User>>(this.profileBase)
      .pipe(map(r => r.data!));
  }

  updateProfile(request: UserUpdateProfileRequest): Observable<User> {
    return this.http
      .put<ApiResponse<User>>(this.profileBase, request)
      .pipe(map(r => r.data!));
  }
}
