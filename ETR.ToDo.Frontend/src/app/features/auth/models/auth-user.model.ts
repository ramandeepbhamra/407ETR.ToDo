import { UserRole } from '../../../core/enums/user-role.enum';

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
}
