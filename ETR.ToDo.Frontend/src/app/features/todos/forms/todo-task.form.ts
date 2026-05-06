import { FormControl } from '@angular/forms';

export interface TodoTaskForm {
  title: FormControl<string>;
  dueDate: FormControl<string | null>;
}
