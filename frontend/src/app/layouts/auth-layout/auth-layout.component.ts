import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, MatIconModule],
  template: `
    <div class="auth-layout">
      <div class="auth-background">
        <div class="auth-shapes">
          <div class="shape shape-1"></div>
          <div class="shape shape-2"></div>
          <div class="shape shape-3"></div>
        </div>
      </div>
      <div class="auth-container">
        <div class="auth-card">
          <div class="auth-header">
            <div class="auth-logo">
              <mat-icon class="logo-icon">engineering</mat-icon>
              <h1 class="auth-title">Contractor Monitoring System</h1>
            </div>
            <p class="auth-subtitle">Government Infrastructure Project Management</p>
          </div>
          <div class="auth-content">
            <router-outlet></router-outlet>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-layout {
      display: flex;
      min-height: 100vh;
      position: relative;
      overflow: hidden;
    }

    .auth-background {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: linear-gradient(135deg, #0d47a1 0%, #1565c0 25%, #1976d2 50%, #1a73e8 75%, #2196f3 100%);
      z-index: 0;
    }

    .auth-shapes {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      overflow: hidden;
    }

    .shape {
      position: absolute;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.05);
    }

    .shape-1 {
      width: 600px;
      height: 600px;
      top: -200px;
      right: -100px;
      animation: float 20s ease-in-out infinite;
    }

    .shape-2 {
      width: 400px;
      height: 400px;
      bottom: -150px;
      left: -100px;
      animation: float 25s ease-in-out infinite reverse;
    }

    .shape-3 {
      width: 300px;
      height: 300px;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      animation: float 18s ease-in-out infinite 2s;
    }

    @keyframes float {
      0%, 100% { transform: translate(0, 0) rotate(0deg); }
      33% { transform: translate(30px, -30px) rotate(5deg); }
      66% { transform: translate(-20px, 20px) rotate(-3deg); }
    }

    .auth-container {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
      position: relative;
      z-index: 1;
    }

    .auth-card {
      width: 100%;
      max-width: 480px;
      background: white;
      border-radius: 16px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15), 0 8px 20px rgba(0, 0, 0, 0.1);
      overflow: hidden;
      animation: slideIn 0.5s ease-out;
    }

    @keyframes slideIn {
      from {
        opacity: 0;
        transform: translateY(30px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .auth-header {
      background: linear-gradient(135deg, #0d47a1, #1a73e8);
      padding: 32px 32px 24px;
      text-align: center;
      color: white;
    }

    .auth-logo {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      margin-bottom: 8px;
    }

    .logo-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #ffd54f;
    }

    .auth-title {
      font-size: 1.5rem;
      font-weight: 700;
      margin: 0;
      letter-spacing: -0.5px;
    }

    .auth-subtitle {
      font-size: 0.875rem;
      opacity: 0.9;
      margin: 0;
      font-weight: 300;
    }

    .auth-content {
      padding: 32px;
    }

    @media (max-width: 600px) {
      .auth-container {
        padding: 16px;
        align-items: flex-start;
        padding-top: 32px;
      }

      .auth-card {
        border-radius: 12px;
      }

      .auth-header {
        padding: 24px 24px 20px;
      }

      .auth-content {
        padding: 24px;
      }

      .auth-title {
        font-size: 1.25rem;
      }

      .logo-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
      }
    }
  `]
})
export class AuthLayoutComponent {}