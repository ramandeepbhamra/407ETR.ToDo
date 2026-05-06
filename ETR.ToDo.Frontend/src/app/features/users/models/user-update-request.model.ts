import { UserRole } from '../../../core/enums/user-role.enum';

export interface UserUpdateRequest {
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  isActive: boolean;
  role: UserRole;
}
