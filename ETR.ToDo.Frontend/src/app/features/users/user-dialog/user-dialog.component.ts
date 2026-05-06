import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { NgxMaskDirective } from 'ngx-mask';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UserService } from '../services/user.service';
import { UserForm } from '../forms/user.form';
import { UserDialogData } from '../models/user-dialog-data.model';
import { UserRole } from '../../../core/enums/user-role.enum';
import { noWhitespaceValidator } from '../../../core/validators/no-whitespace.validator';

/**
 * Modal dialog for creating and editing user accounts (Admin only).
 *
 * Mode is determined by `UserDialogData.userId`:
 * - `null` → create mode: password field is required and shown.
 * - non-null → edit mode: user data is loaded on init; password field is hidden.
 *
 * System users (`isSystemUser === true`) have their role field disabled to prevent
 * accidental role changes on seed/service accounts.
 *
 * Closes with `true` on a successful save so the parent list can reload.
 */
@Component({
  selector: 'app-user-dialog',
  templateUrl: './user-dialog.component.html',
  styleUrl: './user-dialog.component.scss',
  imports: [
    ReactiveFormsModule,
    NgxMaskDirective,
    MatDialogModule,
    TrimOnBlurDirective,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIcon,
    MatInput,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTooltipModule,
  ],
})
export class UserDialogComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly dialogRef = inject(MatDialogRef<UserDialogComponent>);
  readonly data = inject<UserDialogData>(MAT_DIALOG_DATA);

  readonly roles = Object.values(UserRole);
  /** `true` when a `userId` was provided — drives template visibility and submit logic. */
  readonly isEditMode = computed(() => !!this.data.userId);
  /** `true` when the loaded user is a system/seed account — disables the role selector. */
  readonly isSystemUser = signal(false);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  readonly form = new FormGroup<UserForm>({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100), noWhitespaceValidator],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100), noWhitespaceValidator],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(256)],
    }),
    phoneNumber: new FormControl<string | null>(null, {
      validators: [Validators.maxLength(14)],
    }),
    role: new FormControl<UserRole | null>(null, {
      validators: [Validators.required],
    }),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  /** Separate control — only validated and submitted in create mode. */
  readonly passwordControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(6), Validators.maxLength(100)],
  });

  ngOnInit(): void {
    if (this.data.userId) {
      this.loadUser(this.data.userId);
    }
  }

  private loadUser(id: string): void {
    this.isLoading.set(true);
    this.userService.getById(id).subscribe({
      next: user => {
        this.isSystemUser.set(user.isSystemUser);
        this.form.patchValue({
          firstName:   user.firstName,
          lastName:    user.lastName,
          email:       user.email,
          phoneNumber: user.phoneNumber,
          role:        user.role,
          isActive:    user.isActive,
        });
        if (user.isSystemUser) {
          this.form.controls.role.disable();
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load user.');
        this.isLoading.set(false);
      },
    });
  }

  /**
   * Validates and submits the form.
   * In create mode, also validates `passwordControl`.
   * Calls update or create on `UserService` depending on `isEditMode`.
   * Closes the dialog with `true` on success so the parent list reloads.
   */
  submit(): void {
    const passwordInvalid = !this.isEditMode() && this.passwordControl.invalid;
    if (this.form.invalid || passwordInvalid) {
      this.form.markAllAsTouched();
      this.passwordControl.markAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    const v = this.form.getRawValue();

    if (this.isEditMode() && this.data.userId) {
      this.userService.update(this.data.userId, {
        firstName:   v.firstName,
        lastName:    v.lastName,
        email:       v.email,
        phoneNumber: v.phoneNumber || null,
        role:        v.role!,
        isActive:    v.isActive,
      }).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err: HttpErrorResponse) => {
          this.errorMessage.set(err.error?.message || 'Failed to save user.');
          this.isSaving.set(false);
        },
      });
    } else {
      this.userService.create({
        firstName:   v.firstName,
        lastName:    v.lastName,
        email:       v.email,
        password:    this.passwordControl.value,
        phoneNumber: v.phoneNumber || null,
        role:        v.role!,
        isActive:    v.isActive,
      }).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err: HttpErrorResponse) => {
          this.errorMessage.set(err.error?.message || 'Failed to create user.');
          this.isSaving.set(false);
        },
      });
    }
  }
}
