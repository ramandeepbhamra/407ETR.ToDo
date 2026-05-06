import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Group-level validator that checks two sibling controls have the same value.
 * Apply to the FormGroup: new FormGroup({...}, { validators: passwordMatchValidator() })
 */
export function passwordMatchValidator(
  passwordField = 'password',
  confirmField = 'confirmPassword'
): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordField)?.value;
    const confirm = group.get(confirmField)?.value;
    return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
  };
}
