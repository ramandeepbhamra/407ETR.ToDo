import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { AuthDialogService } from '../../../core/services/auth-dialog.service';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { AuthLoginForm } from '../forms/auth-login.form';

@Component({
  selector: 'app-auth-login-dialog',
  templateUrl: './auth-login-dialog.component.html',
  styleUrl: './auth-login-dialog.component.scss',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInput,
    MatIcon,
    MatProgressSpinnerModule,
    TrimOnBlurDirective,
  ],
})
export class AuthLoginDialogComponent {
  private readonly authService      = inject(AuthService);
  private readonly authDialogService = inject(AuthDialogService);
  private readonly dialogRef        = inject(MatDialogRef<AuthLoginDialogComponent>);
  private readonly router           = inject(Router);

  readonly form = new FormGroup<AuthLoginForm>({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  readonly isLoading     = signal(false);
  readonly errorMessage  = signal<string | null>(null);
  readonly showPassword  = signal(false);

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.form.getRawValue();

    this.authService.login({ email, password }).subscribe({
      next: () => {
        this.dialogRef.close();
        this.router.navigate(['/todos']);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message || 'Login failed. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  switchToRegister(): void {
    this.authDialogService.openRegister();
  }
}
