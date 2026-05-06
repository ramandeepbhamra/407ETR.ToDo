import { FormControl } from '@angular/forms';

export interface AuthRegisterForm {
  firstName: FormControl<string>;
  lastName: FormControl<string>;
  email: FormControl<string>;
  password: FormControl<string>;
  confirmPassword: FormControl<string>;
  phoneNumber: FormControl<string | null>;
}
