import { UserRole } from '../../../core/enums/user-role.enum';

export interface UserCreateRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  isActive: boolean;
  role: UserRole;
}
