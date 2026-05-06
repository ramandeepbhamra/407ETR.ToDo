import { PagedAndSortedRequest } from '../../../shared/models/paged-request.model';
import { UserRole } from '../../../core/enums/user-role.enum';

export interface UserGetInput extends PagedAndSortedRequest {
  isActive: boolean | null;
  isSystemUser: boolean | null;
  email: string | null;
  name: string | null;
  role: UserRole | null;
}
