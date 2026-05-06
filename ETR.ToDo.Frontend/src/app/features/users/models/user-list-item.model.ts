import { UserRole } from '../../../core/enums/user-role.enum';

export interface UserListItem {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  isActive: boolean;
  isSystemUser: boolean;
  role: UserRole;
}
