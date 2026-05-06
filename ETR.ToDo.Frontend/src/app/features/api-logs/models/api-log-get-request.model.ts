import { PagedAndSortedRequest } from '../../../shared/models/paged-request.model';

export interface ApiLogGetRequest extends PagedAndSortedRequest {
  startDate: string | null;
  endDate: string | null;
  userName: string | null;
  serviceName: string | null;
  methodName: string | null;
  statusCode: number | null;
  hasException: boolean | null;
  minExecutionDuration: number | null;
  maxExecutionDuration: number | null;
}
