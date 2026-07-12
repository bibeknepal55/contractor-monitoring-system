import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-role-management',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule, MatCardModule,
    MatChipsModule, MatProgressBarModule, MatDividerModule, MatExpansionModule,
    MatCheckboxModule, MatTooltipModule
  ],
  template: `
    <div class="page">
      <div class="header">
        <div><h1>Role & Permission Management</h1><p>Manage roles and their permissions</p></div>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>

      <div class="roles-grid" *ngIf="!loading">
        <mat-card *ngFor="let role of roles" class="role-card">
          <mat-card-header>
            <mat-icon mat-card-avatar [style.color]="roleColor(role.name)">shield</mat-icon>
            <mat-card-title>{{role.name}}</mat-card-title>
            <mat-card-subtitle>{{role.userCount || 0}} users</mat-card-subtitle>
          </mat-card-header>
          <mat-divider></mat-divider>
          <mat-card-content>
            <p style="font-weight:500;margin-bottom:8px;">Permissions ({{role.permissions?.length || 0}}):</p>
            <div style="display:flex;flex-wrap:wrap;gap:4px;">
              <mat-chip *ngFor="let perm of role.permissions" class="perm-chip" 
                style="font-size:0.7rem;height:22px;">
                {{perm}}
              </mat-chip>
              <span *ngIf="!role.permissions || role.permissions.length === 0" style="color:#999;font-size:0.85rem;">No permissions</span>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <div class="empty" *ngIf="!loading && roles.length===0">
        <mat-icon>admin_panel_settings</mat-icon><h3>No roles found</h3>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1400px;margin:0 auto}
    .header{margin-bottom:24px}.header h1{font-size:1.5rem;font-weight:700;margin:0}.header p{color:#666}
    .roles-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(350px,1fr));gap:20px}
    .role-card{border-radius:12px;box-shadow:0 2px 8px rgba(0,0,0,.06);border:1px solid #e8eaed}
    .role-card mat-card-header{padding:20px 20px 0}.role-card mat-card-content{padding:16px 20px 20px}
    mat-divider{margin:12px 0}
    .perm-chip{background:#e8f0fe!important;color:#1967d2!important}
    .empty{text-align:center;padding:64px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;color:#ccc}
    @media(max-width:768px){.page{padding:16px}.roles-grid{grid-template-columns:1fr}}
  `]
})
export class RoleManagementComponent implements OnInit {
  private srv = inject(UserService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);

  roles: any[] = [];
  loading = false;

  ngOnInit(): void {
    // Only SuperAdmin can access this
    if (!this.auth.hasRole('SuperAdmin')) {
      this.notify.error('Access denied. Only SuperAdmin can manage role permissions.');
      return;
    }
    this.fetch();
  }

  fetch(): void {
    this.loading = true;
    this.srv.getRoles().subscribe({
      next: (r: any) => { if (r.success) { this.roles = r.data; } this.loading = false; },
      error: () => { this.loading = false; this.notify.error('Failed to load roles'); }
    });
  }

  roleColor(role: string): string {
    const c: Record<string, string> = {
      'SuperAdmin': '#9c27b0', 'Admin': '#1976d2', 'Test': '#388e3c', 'Viewer': '#757575'
    };
    return c[role] || '#757575';
  }
}