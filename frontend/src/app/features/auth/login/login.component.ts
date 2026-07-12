import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoginRequest } from '../../../core/models/api-response.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login',
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
  templateUrl: './login.component.html',
  styles: [`
    .login-form {
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

    .forgot-password {
      text-align: right;
      margin-top: -8px;
      margin-bottom: 8px;
    }

    .forgot-password a {
      font-size: 0.85rem;
      color: #1976d2;
      text-decoration: none;
      transition: color 0.2s;
    }

    .forgot-password a:hover {
      color: #0d47a1;
    }

    .login-button {
      width: 100%;
      height: 48px;
      font-size: 1rem;
      font-weight: 500;
      letter-spacing: 0.5px;
      margin-top: 8px;
    }

    .register-link {
      text-align: center;
      margin-top: 24px;
      padding-top: 24px;
      border-top: 1px solid #e0e0e0;
    }

    .register-link p {
      color: #757575;
      margin: 0;
      font-size: 0.9rem;
    }

    .register-link a {
      color: #1976d2;
      font-weight: 500;
      text-decoration: none;
      margin-left: 4px;
    }

    .register-link a:hover {
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
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  hidePassword = true;
  isLoading = false;
  loginError: string | null = null;

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.loginError = null;

    const request: LoginRequest = {
      email: this.loginForm.get('email')?.value?.trim(),
      password: this.loginForm.get('password')?.value,
    };

    this.authService
      .login(request)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.authService.setSession(response.data);
            this.notificationService.success('Welcome back!');
            this.router.navigate(['/dashboard']);
          } else {
            this.loginError = response.message || 'Login failed. Please try again.';
          }
        },
        error: (error) => {
          if (error.status === 401) {
            this.loginError = 'Invalid email or password. Please try again.';
          } else if (error.status === 403) {
            this.loginError = 'Your account has been deactivated. Please contact an administrator.';
          } else if (error.error?.message) {
            this.loginError = error.error.message;
          } else {
            this.loginError = 'An error occurred. Please try again later.';
          }
        },
      });
  }

  getEmailErrorMessage(): string {
    const control = this.loginForm.get('email');
    if (control?.hasError('required')) return 'Email is required';
    if (control?.hasError('email')) return 'Please enter a valid email address';
    return '';
  }

  getPasswordErrorMessage(): string {
    const control = this.loginForm.get('password');
    if (control?.hasError('required')) return 'Password is required';
    if (control?.hasError('minlength')) return 'Password must be at least 6 characters';
    return '';
  }
}