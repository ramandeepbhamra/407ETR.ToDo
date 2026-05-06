import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { NgxMaskDirective } from 'ngx-mask';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { UserService } from '../services/user.service';
import { UserForm } from '../forms/user.form';
import { UserRole } from '../../../core/enums/user-role.enum';
import { TrimOnBlurDirective } from '../../../core/directives/trim-on-blur.directive';
import { noWhitespaceValidator } from '../../../core/validators/no-whitespace.validator';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss',
  imports: [
    ReactiveFormsModule,
    NgxMaskDirective,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIcon,
    TrimOnBlurDirective,
    MatInput,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
})
export class UserFormComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly UserRole = UserRole;
  readonly roles = Object.values(UserRole);

  readonly isEditMode = signal(false);
  readonly isSystemUser = signal(false);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private userId: string | null = null;

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

  // Password is only used on add — kept separate so it's not part of UserForm
  readonly passwordControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(6), Validators.maxLength(100)],
  });
  readonly showPassword = signal(false);

  readonly pageTitle = computed(() => this.isEditMode() ? 'Edit user' : 'Add user');

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    if (this.userId) {
      this.isEditMode.set(true);
      this.loadUser(this.userId);
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

    if (this.isEditMode() && this.userId) {
      this.userService.update(this.userId, {
        firstName:   v.firstName,
        lastName:    v.lastName,
        email:       v.email,
        phoneNumber: v.phoneNumber || null,
        role:        v.role!,
        isActive:    v.isActive,
      }).subscribe({
        next: () => this.router.navigate(['/users']),
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
        next: () => this.router.navigate(['/users']),
        error: (err: HttpErrorResponse) => {
          this.errorMessage.set(err.error?.message || 'Failed to create user.');
          this.isSaving.set(false);
        },
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/users']);
  }
}

