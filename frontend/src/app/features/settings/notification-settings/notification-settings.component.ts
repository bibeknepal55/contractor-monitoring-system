import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { HasPermissionDirective } from '../../../core/directives/has-permission.directive';

@Component({
  selector: 'app-notification-settings',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule, MatCardModule,
    MatSlideToggleModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDividerModule, MatTooltipModule, MatChipsModule, LoadingSpinnerComponent,
    HasPermissionDirective
  ],
  template: `
    <div class="page">
      <div class="header">
        <h1>Notification Settings</h1>
        <p>Configure how users receive alerts and updates</p>
      </div>

      <app-loading-spinner *ngIf="loading"></app-loading-spinner>

      <div class="settings-grid" *ngIf="!loading">
        <!-- Email Settings -->
        <mat-card class="settings-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>email</mat-icon>
            <mat-card-title>Email Notifications</mat-card-title>
            <mat-card-subtitle>Configure email delivery settings</mat-card-subtitle>
          </mat-card-header>
          <mat-divider></mat-divider>
          <mat-card-content>
            <mat-form-field appearance="outline">
              <mat-label>SMTP Server</mat-label>
              <input matInput [(ngModel)]="emailSettings.smtpServer" placeholder="smtp.gmail.com">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>SMTP Port</mat-label>
              <input matInput type="number" [(ngModel)]="emailSettings.smtpPort" placeholder="587">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Sender Email</mat-label>
              <input matInput [(ngModel)]="emailSettings.senderEmail" placeholder="noreply@yourcompany.com">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Sender Name</mat-label>
              <input matInput [(ngModel)]="emailSettings.senderName" placeholder="Contractor Monitoring System">
            </mat-form-field>
            <button mat-flat-button color="primary" (click)="saveEmailSettings()" [disabled]="savingEmail">
              <mat-icon>save</mat-icon> {{ savingEmail ? 'Saving...' : 'Save Email Settings' }}
            </button>
            <button mat-stroked-button color="primary" (click)="testEmail()" style="margin-left:8px" [disabled]="testingEmail">
              <mat-icon>send</mat-icon> {{ testingEmail ? 'Sending...' : 'Send Test Email' }}
            </button>
          </mat-card-content>
        </mat-card>

        <!-- Notification Rules -->
        <mat-card class="settings-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>tune</mat-icon>
            <mat-card-title>Notification Rules</mat-card-title>
            <mat-card-subtitle>Choose which events trigger notifications</mat-card-subtitle>
          </mat-card-header>
          <mat-divider></mat-divider>
          <mat-card-content>
            <div class="rule-list">
              <div class="rule-item" *ngFor="let rule of notificationRules">
                <div class="rule-info">
                  <strong>{{ rule.event }}</strong>
                  <span>{{ rule.description }}</span>
                </div>
                <div class="rule-channels">
                  <mat-slide-toggle 
                    color="primary"
                    [checked]="rule.emailEnabled"
                    (change)="rule.emailEnabled = $event.checked; saveRules()">
                    Email
                  </mat-slide-toggle>
                  <mat-slide-toggle 
                    color="primary"
                    [checked]="rule.inAppEnabled"
                    (change)="rule.inAppEnabled = $event.checked; saveRules()">
                    In-App
                  </mat-slide-toggle>
                </div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Webhook Settings -->
        <mat-card class="settings-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>webhook</mat-icon>
            <mat-card-title>Webhook Integration</mat-card-title>
            <mat-card-subtitle>Send events to external systems</mat-card-subtitle>
          </mat-card-header>
          <mat-divider></mat-divider>
          <mat-card-content>
            <div class="webhook-list">
              <div class="webhook-item" *ngFor="let webhook of webhooks; let i = index">
                <mat-form-field appearance="outline">
                  <mat-label>Webhook Name</mat-label>
                  <input matInput [(ngModel)]="webhook.name" placeholder="e.g., ERP Integration">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Webhook URL</mat-label>
                  <input matInput [(ngModel)]="webhook.url" placeholder="https://yourapi.com/webhook">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Secret Key</mat-label>
                  <input matInput [(ngModel)]="webhook.secret" type="password" placeholder="whsec_...">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Events</mat-label>
                  <mat-select [(ngModel)]="webhook.events" multiple>
                    <mat-option value="project.created">Project Created</mat-option>
                    <mat-option value="project.updated">Project Updated</mat-option>
                    <mat-option value="approval.requested">Approval Requested</mat-option>
                    <mat-option value="approval.decided">Approval Decided</mat-option>
                    <mat-option value="financial.updated">Financial Updated</mat-option>
                    <mat-option value="user.created">User Created</mat-option>
                  </mat-select>
                </mat-form-field>
                <div class="webhook-actions">
                  <mat-slide-toggle color="primary" [checked]="webhook.isActive" (change)="webhook.isActive = $event.checked">
                    {{ webhook.isActive ? 'Active' : 'Inactive' }}
                  </mat-slide-toggle>
                  <button mat-icon-button color="warn" (click)="removeWebhook(i)" matTooltip="Remove">
                    <mat-icon>delete</mat-icon>
                  </button>
                  <button mat-icon-button color="primary" (click)="testWebhook(i)" matTooltip="Test">
                    <mat-icon>play_arrow</mat-icon>
                  </button>
                </div>
                <mat-divider *ngIf="i < webhooks.length - 1"></mat-divider>
              </div>
            </div>
            <button mat-stroked-button color="primary" (click)="addWebhook()">
              <mat-icon>add</mat-icon> Add Webhook
            </button>
            <button mat-flat-button color="primary" (click)="saveWebhooks()" 
              [disabled]="savingWebhooks" *ngIf="webhooks.length > 0" style="margin-left:8px">
              <mat-icon>save</mat-icon> Save Webhooks
            </button>
          </mat-card-content>
        </mat-card>

        <!-- Role-Based Notification Settings -->
        <mat-card class="settings-card" *appHasPermission="'RoleManagement.Update'">
          <mat-card-header>
            <mat-icon mat-card-avatar>admin_panel_settings</mat-icon>
            <mat-card-title>Role-Based Notifications</mat-card-title>
            <mat-card-subtitle>Configure which roles receive which notifications</mat-card-subtitle>
          </mat-card-header>
          <mat-divider></mat-divider>
          <mat-card-content>
            <div class="role-notif-grid">
              <div class="role-notif-row header-row">
                <span>Event</span>
                <mat-chip>SuperAdmin</mat-chip>
                <mat-chip>Admin</mat-chip>
                <mat-chip>Viewer</mat-chip>
              </div>
              <div class="role-notif-row" *ngFor="let rn of roleNotifications">
                <span>{{ rn.event }}</span>
                <mat-slide-toggle color="primary" [checked]="rn.superAdmin" (change)="rn.superAdmin = $event.checked"></mat-slide-toggle>
                <mat-slide-toggle color="primary" [checked]="rn.admin" (change)="rn.admin = $event.checked"></mat-slide-toggle>
                <mat-slide-toggle color="primary" [checked]="rn.viewer" (change)="rn.viewer = $event.checked"></mat-slide-toggle>
              </div>
            </div>
            <button mat-flat-button color="primary" (click)="saveRoleNotifications()" style="margin-top:12px">
              <mat-icon>save</mat-icon> Save Role Notifications
            </button>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1200px;margin:0 auto}
    .header{margin-bottom:24px}.header h1{font-size:1.5rem;font-weight:700;margin:0}.header p{color:#666;font-size:.85rem}
    .settings-grid{display:flex;flex-direction:column;gap:20px}
    .settings-card{border-radius:12px;border:1px solid #e5e7eb;box-shadow:0 1px 3px rgba(0,0,0,.04)}
    .settings-card mat-card-header{padding:16px 20px 0}
    .settings-card mat-card-content{padding:16px 20px 20px}
    mat-form-field{width:100%}

    .rule-list{display:flex;flex-direction:column;gap:4px}
    .rule-item{display:flex;justify-content:space-between;align-items:center;padding:12px 0;border-bottom:1px solid #f5f5f5}
    .rule-item:last-child{border-bottom:none}
    .rule-info{flex:1}.rule-info strong{display:block;font-size:.85rem;color:#333}.rule-info span{font-size:.75rem;color:#999}
    .rule-channels{display:flex;gap:16px}

    .webhook-list{display:flex;flex-direction:column;gap:8px;margin-bottom:16px}
    .webhook-item{padding:8px 0}
    .webhook-actions{display:flex;align-items:center;gap:8px;justify-content:flex-end}

    .role-notif-grid{border:1px solid #e5e7eb;border-radius:8px;overflow:hidden}
    .role-notif-row{display:grid;grid-template-columns:2fr 1fr 1fr 1fr;align-items:center;padding:12px 16px;border-bottom:1px solid #f5f5f5}
    .role-notif-row:last-child{border-bottom:none}
    .role-notif-row.header-row{background:#f9fafb;font-weight:600;font-size:.8rem;color:#666}
    .role-notif-row span{font-size:.82rem;color:#333}

    @media(max-width:768px){
      .page{padding:16px}
      .role-notif-row{grid-template-columns:1fr 1fr 1fr 1fr;font-size:.7rem}
      .rule-item{flex-direction:column;align-items:flex-start;gap:8px}
    }
  `]
})
export class NotificationSettingsComponent implements OnInit {
  private notifySrv = inject(NotificationService);
  
  loading = false;
  savingEmail = false;
  testingEmail = false;
  savingWebhooks = false;

  emailSettings = {
    smtpServer: 'smtp.gmail.com',
    smtpPort: 587,
    senderEmail: 'noreply@contractor.gov.np',
    senderName: 'Contractor Monitoring System',
  };

  notificationRules = [
    { event: 'Approval Requested', description: 'When someone submits an approval request', emailEnabled: true, inAppEnabled: true },
    { event: 'Approval Decided', description: 'When an approval is approved or rejected', emailEnabled: true, inAppEnabled: true },
    { event: 'Project Created', description: 'When a new project is added', emailEnabled: false, inAppEnabled: true },
    { event: 'Project Updated', description: 'When project details change', emailEnabled: false, inAppEnabled: true },
    { event: 'Financial Updated', description: 'When contract financial data changes', emailEnabled: true, inAppEnabled: true },
    { event: 'User Created', description: 'When a new user account is created', emailEnabled: true, inAppEnabled: false },
    { event: 'Role Changed', description: 'When user permissions change', emailEnabled: true, inAppEnabled: true },
    { event: 'Time Extension Requested', description: 'When time extension is requested', emailEnabled: true, inAppEnabled: true },
    { event: 'Bond Expiring', description: 'When a performance bond is about to expire', emailEnabled: true, inAppEnabled: true },
    { event: 'System Alert', description: 'Critical system-level notifications', emailEnabled: true, inAppEnabled: true },
  ];

  webhooks: any[] = [
    { name: 'ERP System', url: '', secret: '', events: ['project.created', 'financial.updated'], isActive: true },
  ];

  roleNotifications = [
    { event: 'Approval Requested', superAdmin: true, admin: true, viewer: false },
    { event: 'Project Created', superAdmin: true, admin: true, viewer: true },
    { event: 'Financial Updated', superAdmin: true, admin: true, viewer: false },
    { event: 'User Created', superAdmin: true, admin: false, viewer: false },
    { event: 'System Alert', superAdmin: true, admin: true, viewer: false },
  ];

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading = true;
    // Simulate loading
    setTimeout(() => { this.loading = false; }, 500);
  }

  saveEmailSettings(): void {
    this.savingEmail = true;
    setTimeout(() => {
      this.savingEmail = false;
      this.notifySrv.success('Email settings saved');
    }, 800);
  }

  testEmail(): void {
    this.testingEmail = true;
    setTimeout(() => {
      this.testingEmail = false;
      this.notifySrv.success('Test email sent! Check your inbox.');
    }, 1000);
  }

  saveRules(): void {
    this.notifySrv.info('Notification rules updated');
  }

  addWebhook(): void {
    this.webhooks.push({ name: '', url: '', secret: '', events: [], isActive: true });
  }

  removeWebhook(index: number): void {
    this.webhooks.splice(index, 1);
  }

  saveWebhooks(): void {
    this.savingWebhooks = true;
    setTimeout(() => {
      this.savingWebhooks = false;
      this.notifySrv.success('Webhooks saved');
    }, 800);
  }

  testWebhook(index: number): void {
    const webhook = this.webhooks[index];
    if (!webhook.url) {
      this.notifySrv.error('Webhook URL is required');
      return;
    }
    this.notifySrv.info(`Test webhook sent to ${webhook.name}`);
  }

  saveRoleNotifications(): void {
    this.notifySrv.success('Role notification preferences saved');
  }
}