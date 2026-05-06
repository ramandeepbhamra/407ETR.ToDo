import { Entity } from '../../../shared/models/entity.model';
import { UserRole } from '../../../core/enums/user-role.enum';

export interface User extends Entity {
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber: string | null;
  isActive: boolean;
  isSystemUser: boolean;
  role: UserRole;
  createdDate: string;
  createdBy: string | null;
  modifiedDate: string | null;
  modifiedBy: string | null;
}
