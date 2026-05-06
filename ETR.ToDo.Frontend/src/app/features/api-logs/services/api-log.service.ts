import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { ApiLog } from '../models/api-log.model';
import { ApiLogGetRequest } from '../models/api-log-get-request.model';

@Injectable({ providedIn: 'root' })
export class ApiLogService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/logs`;

  getLogs(request: ApiLogGetRequest): Observable<PagedResult<ApiLog>> {
    let params = new HttpParams()
      .set('page', request.page)
      .set('pageSize', request.pageSize)
      .set('sortDirection', request.sortDirection);

    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.startDate) params = params.set('startDate', request.startDate);
    if (request.endDate) params = params.set('endDate', request.endDate);
    if (request.userName) params = params.set('userName', request.userName);
    if (request.serviceName) params = params.set('serviceName', request.serviceName);
    if (request.methodName) params = params.set('methodName', request.methodName);
    if (request.statusCode != null) params = params.set('statusCode', request.statusCode);
    if (request.hasException != null) params = params.set('hasException', request.hasException);
    if (request.minExecutionDuration != null) params = params.set('minExecutionDuration', request.minExecutionDuration);
    if (request.maxExecutionDuration != null) params = params.set('maxExecutionDuration', request.maxExecutionDuration);

    return this.http
      .get<ApiResponse<PagedResult<ApiLog>>>(this.base, { params })
      .pipe(map(r => r.data!));
  }
}
