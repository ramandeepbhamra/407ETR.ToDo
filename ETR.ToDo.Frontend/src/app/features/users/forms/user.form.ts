import { FormControl } from '@angular/forms';
import { UserRole } from '../../../core/enums/user-role.enum';

export interface UserForm {
  firstName: FormControl<string>;
  lastName: FormControl<string>;
  email: FormControl<string>;
  phoneNumber: FormControl<string | null>;
  role: FormControl<UserRole | null>;
  isActive: FormControl<boolean>;
}
