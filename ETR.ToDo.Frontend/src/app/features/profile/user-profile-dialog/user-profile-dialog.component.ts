import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { NgxMaskDirective } from 'ngx-mask';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../users/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { noWhitespaceValidator } from '../../../core/validators/no-whitespace.validator';
import { UserProfileForm } from '../forms/user-profile.form';

@Component({
  selector: 'app-user-profile-dialog',
  templateUrl: './user-profile-dialog.component.html',
  styleUrl: './user-profile-dialog.component.scss',
  imports: [
    ReactiveFormsModule,
    NgxMaskDirective,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInput,
    MatIcon,
    MatProgressSpinnerModule,
    TrimOnBlurDirective,
  ],
})
export class UserProfileDialogComponent implements OnInit {
  private readonly userService  = inject(UserService);
  private readonly authService  = inject(AuthService);
  private readonly dialogRef    = inject(MatDialogRef<UserProfileDialogComponent>);

  readonly userRole = computed(() => this.authService.currentUser()?.role);

  readonly form = new FormGroup<UserProfileForm>({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100), noWhitespaceValidator],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100), noWhitespaceValidator],
    }),
    phoneNumber: new FormControl<string | null>(null, {
      validators: [Validators.maxLength(20)],
    }),
  });

  readonly isLoading    = signal(true);
  readonly isSaving     = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.userService.getProfile().subscribe({
      next: user => {
        this.form.patchValue({
          firstName:   user.firstName,
          lastName:    user.lastName,
          phoneNumber: user.phoneNumber,
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load profile.');
        this.isLoading.set(false);
      },
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const { firstName, lastName, phoneNumber } = this.form.getRawValue();

    this.userService.updateProfile({ firstName, lastName, phoneNumber: phoneNumber ?? null }).subscribe({
      next: (updatedUser) => {
        // Update the current user in AuthService to refresh the navigation bar
        const currentUser = this.authService.currentUser();
        if (currentUser) {
          this.authService.updateCurrentUser({
            ...currentUser,
            fullName: `${updatedUser.firstName} ${updatedUser.lastName}`,
          });
        }
        this.dialogRef.close(true);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message || 'Failed to update profile. Please try again.');
        this.isSaving.set(false);
      },
    });
  }
}
