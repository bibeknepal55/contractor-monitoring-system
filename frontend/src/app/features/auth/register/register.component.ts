import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RegisterRequest } from '../../../core/models/api-response.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    RouterLink,
  ],
  templateUrl: './register.component.html',
  styles: [`
    .register-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
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

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    @media (max-width: 500px) {
      .form-row {
        grid-template-columns: 1fr;
      }
    }

    .register-button {
      width: 100%;
      height: 48px;
      font-size: 1rem;
      font-weight: 500;
      letter-spacing: 0.5px;
      margin-top: 8px;
    }

    .login-link {
      text-align: center;
      margin-top: 24px;
      padding-top: 24px;
      border-top: 1px solid #e0e0e0;
    }

    .login-link p {
      color: #757575;
      margin: 0;
      font-size: 0.9rem;
    }

    .login-link a {
      color: #1976d2;
      font-weight: 500;
      text-decoration: none;
      margin-left: 4px;
    }

    .login-link a:hover {
      text-decoration: underline;
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
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  registerForm: FormGroup = this.fb.group(
    {
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.pattern('^[+]?[0-9]{7,15}$')]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).{8,}$/)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: this.passwordMatchValidator }
  );

  hidePassword = true;
  hideConfirmPassword = true;
  isLoading = false;
  registerError: string | null = null;

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    if (password && confirmPassword && password !== confirmPassword) {
      control.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.registerError = null;

    const request: RegisterRequest = {
      email: this.registerForm.get('email')?.value?.trim(),
      password: this.registerForm.get('password')?.value,
      confirmPassword: this.registerForm.get('confirmPassword')?.value,
      firstName: this.registerForm.get('firstName')?.value?.trim(),
      lastName: this.registerForm.get('lastName')?.value?.trim(),
      phoneNumber: this.registerForm.get('phoneNumber')?.value?.trim() || undefined,
    };

    this.authService
      .register(request)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.authService.setSession(response.data);
            this.notificationService.success('Account created successfully!');
            this.router.navigate(['/dashboard']);
          } else {
            this.registerError = response.message || 'Registration failed. Please try again.';
          }
        },
        error: (error) => {
          if (error.status === 409) {
            this.registerError = 'An account with this email already exists.';
          } else if (error.error?.errors?.length) {
            this.registerError = error.error.errors.join('. ');
          } else if (error.error?.message) {
            this.registerError = error.error.message;
          } else {
            this.registerError = 'Registration failed. Please try again later.';
          }
        },
      });
  }

  getFirstNameErrorMessage(): string {
    const control = this.registerForm.get('firstName');
    if (control?.hasError('required')) return 'First name is required';
    if (control?.hasError('minlength')) return 'First name must be at least 2 characters';
    if (control?.hasError('maxlength')) return 'First name must be at most 50 characters';
    return '';
  }

  getLastNameErrorMessage(): string {
    const control = this.registerForm.get('lastName');
    if (control?.hasError('required')) return 'Last name is required';
    if (control?.hasError('minlength')) return 'Last name must be at least 2 characters';
    if (control?.hasError('maxlength')) return 'Last name must be at most 50 characters';
    return '';
  }

  getEmailErrorMessage(): string {
    const control = this.registerForm.get('email');
    if (control?.hasError('required')) return 'Email is required';
    if (control?.hasError('email')) return 'Please enter a valid email address';
    return '';
  }

  getPhoneErrorMessage(): string {
    const control = this.registerForm.get('phoneNumber');
    if (control?.hasError('pattern')) return 'Please enter a valid phone number (7-15 digits, optional + prefix)';
    return '';
  }

  getPasswordErrorMessage(): string {
    const control = this.registerForm.get('password');
    if (control?.hasError('required')) return 'Password is required';
    if (control?.hasError('minlength')) return 'Password must be at least 8 characters';
    if (control?.hasError('pattern')) return 'Password must contain uppercase, lowercase, number, and special character';
    return '';
  }

  getConfirmPasswordErrorMessage(): string {
    const control = this.registerForm.get('confirmPassword');
    if (control?.hasError('required')) return 'Please confirm your password';
    if (control?.hasError('passwordMismatch')) return 'Passwords do not match';
    return '';
  }
}