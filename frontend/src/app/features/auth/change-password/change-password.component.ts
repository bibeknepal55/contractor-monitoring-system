import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ChangePasswordRequest } from '../../../core/models/api-response.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './change-password.component.html',
  styles: [`
    .change-password-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
      max-width: 480px;
      margin: 0 auto;
    }

    .form-header {
      text-align: center;
      margin-bottom: 8px;
    }

    .form-header h2 {
      font-size: 1.5rem;
      font-weight: 600;
      color: #212121;
      margin: 0 0 4px;
    }

    .form-header p {
      color: #757575;
      margin: 0;
      font-size: 0.9rem;
    }

    mat-form-field {
      width: 100%;
    }

    .form-actions {
      display: flex;
      gap: 16px;
      margin-top: 8px;
    }

    .form-actions button {
      flex: 1;
      height: 48px;
      font-weight: 500;
    }

    .success-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      background: #e8f5e9;
      border-radius: 8px;
      color: #2e7d32;
      font-size: 0.875rem;
      margin-bottom: 8px;
    }

    .success-message mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      background: #ffebee;
      border-radius: 8px;
      color: #c62828;
      font-size: 0.875rem;
      margin-bottom: 8px;
    }

    .error-message mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }
  `]
})
export class ChangePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  changePasswordForm: FormGroup = this.fb.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).{8,}$/)]],
      confirmNewPassword: ['', [Validators.required]],
    },
    { validators: this.passwordMatchValidator }
  );

  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;
  isLoading = false;
  isSuccess = false;
  changePasswordError: string | null = null;

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmNewPassword = control.get('confirmNewPassword')?.value;
    if (newPassword !== confirmNewPassword) {
      control.get('confirmNewPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.changePasswordError = null;
    this.isSuccess = false;

    const request: ChangePasswordRequest = {
      currentPassword: this.changePasswordForm.get('currentPassword')?.value,
      newPassword: this.changePasswordForm.get('newPassword')?.value,
      confirmNewPassword: this.changePasswordForm.get('confirmNewPassword')?.value,
    };

    this.authService
      .changePassword(request)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.isSuccess = true;
            this.changePasswordForm.reset();
            this.notificationService.success('Password changed successfully!');
            setTimeout(() => this.router.navigate(['/dashboard']), 2000);
          } else {
            this.changePasswordError = response.message || 'Failed to change password.';
          }
        },
        error: (error) => {
          if (error.status === 400) {
            this.changePasswordError = 'Current password is incorrect.';
          } else if (error.error?.errors?.length) {
            this.changePasswordError = error.error.errors.join('. ');
          } else if (error.error?.message) {
            this.changePasswordError = error.error.message;
          } else {
            this.changePasswordError = 'Failed to change password. Please try again.';
          }
        },
      });
  }

  onCancel(): void {
    this.router.navigate(['/dashboard']);
  }

  getCurrentPasswordErrorMessage(): string {
    const control = this.changePasswordForm.get('currentPassword');
    if (control?.hasError('required')) return 'Current password is required';
    return '';
  }

  getNewPasswordErrorMessage(): string {
    const control = this.changePasswordForm.get('newPassword');
    if (control?.hasError('required')) return 'New password is required';
    if (control?.hasError('minlength')) return 'Password must be at least 8 characters';
    if (control?.hasError('pattern')) return 'Password must contain uppercase, lowercase, number, and special character';
    return '';
  }

  getConfirmPasswordErrorMessage(): string {
    const control = this.changePasswordForm.get('confirmNewPassword');
    if (control?.hasError('required')) return 'Please confirm your new password';
    if (control?.hasError('passwordMismatch')) return 'Passwords do not match';
    return '';
  }
}