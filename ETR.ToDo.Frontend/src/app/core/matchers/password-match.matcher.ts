import { FormControl, FormGroupDirective, NgForm } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';

/**
 * Custom error state matcher for a confirm-password field.
 * Activates the Material error state when the control is touched AND either:
 *  - the control itself is invalid (e.g. required), or
 *  - the parent FormGroup carries a passwordMismatch error.
 */
export class PasswordMatchErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, _form: FormGroupDirective | NgForm | null): boolean {
    if (!control?.touched) return false;
    return control.invalid || (control.parent?.hasError('passwordMismatch') ?? false);
  }
}
