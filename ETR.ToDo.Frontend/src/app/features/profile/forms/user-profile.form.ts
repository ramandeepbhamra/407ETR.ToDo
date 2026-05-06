import { FormControl } from '@angular/forms';

export interface UserProfileForm {
  firstName:   FormControl<string>;
  lastName:    FormControl<string>;
  phoneNumber: FormControl<string | null>;
}
