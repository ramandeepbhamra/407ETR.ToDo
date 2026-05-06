import { FormControl } from '@angular/forms';

export interface AuthLoginForm {
  email: FormControl<string>;
  password: FormControl<string>;
}
