import { AbstractControl, ValidationErrors } from '@angular/forms';

/**
 * Rejects values that are non-empty but consist entirely of whitespace.
 * Returns { required: true } so it collapses into the existing required error message.
 */
export function noWhitespaceValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string;
  return value && !value.trim() ? { required: true } : null;
}
