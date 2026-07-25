import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProfileService } from '../../../core/services/profile.service';
import { NotificationService } from '../../../core/services/notification.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-mfa-setup',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule, MatCardModule,
    MatFormFieldModule, MatInputModule, MatDividerModule, MatProgressBarModule,
    MatSlideToggleModule, MatTooltipModule
  ],
  template: `
    <mat-card class="mfa-card">
      <mat-card-header>
        <mat-icon mat-card-avatar [style.color]="mfaEnabled ? '#059669' : '#f57c00'">
          {{ mfaEnabled ? 'security' : 'security_update_warning' }}
        </mat-icon>
        <mat-card-title>Two-Factor Authentication (2FA)</mat-card-title>
        <mat-card-subtitle>
          {{ mfaEnabled ? 'Your account is protected with 2FA' : 'Add an extra layer of security to your account' }}
        </mat-card-subtitle>
      </mat-card-header>
      <mat-divider></mat-divider>
      <mat-card-content>
        <div class="mfa-status">
          <div class="status-indicator" [class.enabled]="mfaEnabled" [class.disabled]="!mfaEnabled">
            <mat-icon>{{ mfaEnabled ? 'check_circle' : 'error' }}</mat-icon>
            <span>{{ mfaEnabled ? '2FA is ENABLED' : '2FA is DISABLED' }}</span>
          </div>
        </div>

        <!-- Setup Flow -->
        <div class="setup-flow" *ngIf="!mfaEnabled && !showSetup">
          <p class="setup-info">
            Two-factor authentication adds an extra layer of security. After entering your password,
            you'll need to enter a 6-digit code from your authenticator app.
          </p>
          <button mat-flat-button color="primary" (click)="startSetup()">
            <mat-icon>security</mat-icon> Set Up 2FA
          </button>
        </div>

        <!-- Setup Steps -->
        <div class="setup-steps" *ngIf="showSetup">
          <mat-progress-bar *ngIf="settingUp" mode="indeterminate" color="primary"></mat-progress-bar>

          <!-- Step 1: Enter Password -->
          <div class="step" *ngIf="setupStep === 1">
            <h3>Step 1: Verify Password</h3>
            <mat-form-field appearance="outline">
              <mat-label>Current Password</mat-label>
              <input matInput type="password" [(ngModel)]="password" placeholder="Enter your password">
            </mat-form-field>
            <button mat-flat-button color="primary" (click)="verifyPassword()" [disabled]="!password">
              Verify & Continue
            </button>
          </div>

          <!-- Step 2: Scan QR Code -->
          <div class="step" *ngIf="setupStep === 2">
            <h3>Step 2: Scan QR Code</h3>
            <p>Scan this QR code with Google Authenticator, Microsoft Authenticator, or Authy.</p>
            <div class="qr-container">
              <img *ngIf="qrCodeDataUrl" [src]="qrCodeDataUrl" alt="MFA QR Code" class="qr-image">
              <div *ngIf="!qrCodeDataUrl" class="qr-placeholder">
                <mat-icon>qr_code_2</mat-icon>
                <span>Loading QR Code...</span>
              </div>
            </div>
            <mat-form-field appearance="outline">
              <mat-label>Or enter setup key manually</mat-label>
              <input matInput [value]="setupKey" readonly>
              <button mat-icon-button matSuffix (click)="copySetupKey()" matTooltip="Copy">
                <mat-icon>content_copy</mat-icon>
              </button>
            </mat-form-field>
            <button mat-flat-button color="primary" (click)="setupStep = 3">
              I've scanned the code → Continue
            </button>
          </div>

          <!-- Step 3: Verify Code -->
          <div class="step" *ngIf="setupStep === 3">
            <h3>Step 3: Verify Setup</h3>
            <p>Enter the 6-digit code from your authenticator app to verify.</p>
            <mat-form-field appearance="outline">
              <mat-label>6-Digit Code</mat-label>
              <input matInput [(ngModel)]="verificationCode" maxlength="6" placeholder="000000">
            </mat-form-field>
            <button mat-flat-button color="primary" (click)="verifyAndEnable()" 
              [disabled]="!verificationCode || verificationCode.length < 6 || settingUp">
              {{ settingUp ? 'Verifying...' : 'Verify & Enable 2FA' }}
            </button>
          </div>
        </div>

        <!-- Disable 2FA -->
        <div class="disable-section" *ngIf="mfaEnabled">
          <p class="warning-text">
            ⚠️ Disabling 2FA will make your account less secure.
          </p>
          <button mat-stroked-button color="warn" (click)="disableMfa()">
            <mat-icon>security_update_warning</mat-icon> Disable 2FA
          </button>
        </div>

        <!-- Backup Codes -->
        <div class="backup-section" *ngIf="mfaEnabled && backupCodes.length > 0">
          <h4>Backup Recovery Codes</h4>
          <p>Save these codes in a safe place. Each code can be used once.</p>
          <div class="backup-codes">
            <code *ngFor="let code of backupCodes">{{ code }}</code>
          </div>
          <button mat-button color="primary" (click)="downloadBackupCodes()">
            <mat-icon>download</mat-icon> Download Codes
          </button>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .mfa-card{border-radius:12px;border:1px solid #e5e7eb;margin-bottom:20px}
    .mfa-card mat-card-header{padding:16px 20px 0}
    .mfa-card mat-card-content{padding:16px 20px 20px}
    .mfa-status{margin-bottom:16px}
    .status-indicator{display:flex;align-items:center;gap:8px;padding:10px 16px;border-radius:8px;font-weight:600;font-size:.9rem}
    .status-indicator.enabled{background:#e6f4ea;color:#137333}
    .status-indicator.disabled{background:#fef7e0;color:#b06000}
    .setup-info{font-size:.85rem;color:#666;line-height:1.5;margin-bottom:16px}
    .step{padding:8px 0}.step h3{font-size:.95rem;font-weight:600;color:#333;margin:0 0 8px}
    .step p{font-size:.82rem;color:#666;margin:0 0 12px}
    .qr-container{margin-bottom:16px}
    .qr-image{width:200px;height:200px;border-radius:8px;border:1px solid #e5e7eb}
    .warning-text{font-size:.82rem;color:#dc2626;margin-bottom:12px}
    .backup-section{margin-top:20px}.backup-section h4{font-size:.9rem;font-weight:600;color:#333;margin:0 0 4px}
    .backup-section p{font-size:.8rem;color:#666;margin:0 0 8px}
    .backup-codes{display:grid;grid-template-columns:repeat(3,1fr);gap:8px;margin-bottom:12px}
    .backup-codes code{background:#f5f5f5;padding:8px;border-radius:6px;text-align:center;font-family:monospace;font-size:.85rem}
    mat-form-field{width:100%}
    @media(max-width:768px){.backup-codes{grid-template-columns:repeat(2,1fr)}}
  `]
})
export class MfaSetupComponent {
  @Input() mfaEnabled = false;
  @Output() mfaChanged = new EventEmitter<boolean>();

  private profileSrv = inject(ProfileService);
  private notify = inject(NotificationService);

  showSetup = false;
  setupStep = 1;
  password = '';
  setupKey = '';
  qrCodeDataUrl = '';
  verificationCode = '';
  settingUp = false;
  backupCodes: string[] = [];

  startSetup(): void {
    this.showSetup = true;
    this.setupStep = 1;
  }

  verifyPassword(): void {
    this.settingUp = true;
    this.profileSrv.updateTwoFactor({ enable: false, password: this.password }).subscribe({
      next: (r: any) => {
        this.settingUp = false;
        if (r.success) {
          this.setupKey = r.data?.secret || r.data?.setupKey || '';
          this.qrCodeDataUrl = r.data?.qrCode || '';
          this.setupStep = 2;
        } else {
          this.notify.error(r.message || 'Invalid password');
        }
      },
      error: () => { this.settingUp = false; this.notify.error('Failed'); }
    });
  }

  copySetupKey(): void {
    navigator.clipboard.writeText(this.setupKey).then(() => this.notify.info('Setup key copied!'));
  }

  verifyAndEnable(): void {
    this.settingUp = true;
    this.profileSrv.updateTwoFactor({ enable: true, password: this.password, code: this.verificationCode }).subscribe({
      next: (r: any) => {
        this.settingUp = false;
        if (r.success) {
          this.mfaEnabled = true;
                   this.backupCodes = r.data?.backupCodes || [];
          this.showSetup = false;
          this.setupStep = 1;
          this.verificationCode = '';
          this.mfaChanged.emit(true);
          this.notify.success('2FA enabled successfully!');
        } else {
          this.notify.error(r.message || 'Invalid verification code');
        }
      },
      error: () => { this.settingUp = false; this.notify.error('Failed to enable 2FA'); }
    });
  }

  async disableMfa(): Promise<void> {
    const { value: password } = await Swal.fire({
      title: 'Disable 2FA',
      text: 'Enter your password to disable two-factor authentication.',
      input: 'password',
      inputLabel: 'Password',
      inputPlaceholder: 'Enter your password',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      confirmButtonText: 'Disable 2FA',
      cancelButtonText: 'Cancel',
      inputValidator: (v) => !v ? 'Password is required' : null
    });

    if (!password) return;

    this.profileSrv.updateTwoFactor({ enable: false, password }).subscribe({
      next: (r: any) => {
        if (r.success) {
          this.mfaEnabled = false;
          this.backupCodes = [];
          this.mfaChanged.emit(false);
          this.notify.success('2FA disabled');
        } else {
          this.notify.error(r.message || 'Failed to disable 2FA');
        }
      },
      error: () => this.notify.error('Failed to disable 2FA')
    });
  }

  downloadBackupCodes(): void {
    const text = this.backupCodes.join('\n');
    const blob = new Blob([text], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `2fa-backup-codes-${new Date().toISOString().split('T')[0]}.txt`;
    a.click();
    URL.revokeObjectURL(url);
    this.notify.info('Backup codes downloaded');
  }
}