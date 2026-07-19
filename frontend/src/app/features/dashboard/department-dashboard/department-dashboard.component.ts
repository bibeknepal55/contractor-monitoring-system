import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DashboardService } from '../../../core/services/dashboard.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrganizationService } from '../../../core/services/organization.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';

@Component({
  selector: 'app-department-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatCardModule, MatIconModule, MatButtonModule,
    MatChipsModule, MatDividerModule, MatProgressBarModule, MatTooltipModule,
    LoadingSpinnerComponent, ErrorStateComponent
  ],
  template: `
    <div class="page">
      <div class="welcome-banner">
        <div class="welcome-text">
          <h1>{{ orgName }} Dashboard</h1>
          <p>Welcome back, {{ auth.getCurrentUser()?.firstName }}! Here's your department overview.</p>
        </div>
        <mat-chip class="role-chip">{{ roleName }}</mat-chip>
      </div>

      <app-loading-spinner *ngIf="loading"></app-loading-spinner>
      <app-error-state *ngIf="!loading && error" [message]="error" (retry)="loadDashboard()"></app-error-state>

      <div class="stats-grid" *ngIf="!loading && !error">
        <!-- Quick Stats -->
        <mat-card class="stat-card">
          <mat-icon class="stat-icon" style="color:#1976d2">business</mat-icon>
          <div class="stat-info">
            <strong>{{ stats.totalProjects }}</strong>
            <span>Active Projects</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon class="stat-icon" style="color:#388e3c">group</mat-icon>
          <div class="stat-info">
            <strong>{{ stats.totalUsers }}</strong>
            <span>Department Members</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon class="stat-icon" style="color:#f57c00">receipt_long</mat-icon>
          <div class="stat-info">
            <strong>₹{{ (stats.totalBudget || 0) | number:'1.0-0' }}</strong>
            <span>Total Budget</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon class="stat-icon" style="color:#9c27b0">pending_actions</mat-icon>
          <div class="stat-info">
            <strong>{{ stats.pendingApprovals }}</strong>
            <span>Pending Approvals</span>
          </div>
        </mat-card>
      </div>

      <!-- Pending Actions -->
      <div class="section-grid">
        <mat-card class="section-card">
          <div class="section-header">
            <h3>Pending Approvals</h3>
            <button mat-button color="primary" routerLink="/approvals">View All</button>
          </div>
          <mat-divider></mat-divider>
          <div class="approval-list">
            <div class="approval-item" *ngFor="let a of pendingApprovals">
              <mat-icon [style.color]="getPriorityColor(a.priority)">{{ getPriorityIcon(a.priority) }}</mat-icon>
              <div class="approval-info">
                <strong>{{ a.title }}</strong>
                <span>{{ a.requestedBy }} · {{ formatTimeAgo(a.requestedAt) }}</span>
              </div>
              <div class="approval-actions">
                <button mat-icon-button color="primary" (click)="approveItem(a)" matTooltip="Approve">
                  <mat-icon>check_circle</mat-icon>
                </button>
                <button mat-icon-button color="warn" (click)="rejectItem(a)" matTooltip="Reject">
                  <mat-icon>cancel</mat-icon>
                </button>
              </div>
            </div>
            <p class="empty-text" *ngIf="pendingApprovals.length === 0">No pending approvals</p>
          </div>
        </mat-card>

        <!-- Recent Activity -->
        <mat-card class="section-card">
          <div class="section-header">
            <h3>Recent Activity</h3>
            <button mat-button color="primary" routerLink="/user-logs">View All</button>
          </div>
          <mat-divider></mat-divider>
          <div class="activity-list">
            <div class="activity-item" *ngFor="let a of recentActivity">
              <mat-icon>{{ getActivityIcon(a.type) }}</mat-icon>
              <div class="activity-info">
                <strong>{{ a.userName }}</strong>
                <span>{{ a.action }}</span>
                <small>{{ formatTimeAgo(a.timestamp) }}</small>
              </div>
            </div>
            <p class="empty-text" *ngIf="recentActivity.length === 0">No recent activity</p>
          </div>
        </mat-card>

        <!-- My Projects -->
        <mat-card class="section-card">
          <div class="section-header">
            <h3>My Projects</h3>
            <button mat-button color="primary" routerLink="/projects">View All</button>
          </div>
          <mat-divider></mat-divider>
          <div class="project-list">
            <div class="project-item" *ngFor="let p of myProjects">
              <div class="project-status" [style.background]="getStatusColor(p.status)"></div>
              <div class="project-info">
                <strong>{{ p.projectName }}</strong>
                <span>{{ p.projectCode }} · {{ p.status || 'Unknown' }}</span>
              </div>
              <mat-chip class="progress-chip">
                {{ p.progress || 0 }}%
              </mat-chip>
            </div>
            <p class="empty-text" *ngIf="myProjects.length === 0">No projects assigned</p>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1400px;margin:0 auto}
    .welcome-banner{display:flex;justify-content:space-between;align-items:center;margin-bottom:24px;flex-wrap:wrap;gap:12px}
    .welcome-banner h1{font-size:1.6rem;font-weight:700;margin:0}.welcome-banner p{color:#666;font-size:.85rem;margin:2px 0 0}
    .role-chip{background:#e8f0fe!important;color:#1967d2!important;font-weight:600}

    .stats-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:16px;margin-bottom:24px}
    .stat-card{display:flex;align-items:center;gap:16px;padding:20px;border-radius:12px;border:1px solid #e5e7eb;box-shadow:0 1px 3px rgba(0,0,0,.04)}
    .stat-icon{font-size:40px;width:40px;height:40px}
    .stat-info strong{display:block;font-size:1.4rem;font-weight:700;color:#111}
    .stat-info span{font-size:.78rem;color:#888}

    .section-grid{display:grid;grid-template-columns:1fr 1fr;gap:20px}
    .section-card{border-radius:12px;border:1px solid #e5e7eb;box-shadow:0 1px 3px rgba(0,0,0,.04)}
    .section-card:last-child{grid-column:span 2}
    .section-header{display:flex;justify-content:space-between;align-items:center;padding:16px 20px 0}
    .section-header h3{font-size:1rem;font-weight:650;margin:0}

    .approval-list,.activity-list,.project-list{padding:8px 20px 16px;max-height:300px;overflow-y:auto}
    .approval-item,.activity-item{display:flex;align-items:center;gap:12px;padding:10px 0;border-bottom:1px solid #f9fafb}
    .approval-item:last-child,.activity-item:last-child{border-bottom:none}
    .approval-info,.activity-info{flex:1}.approval-info strong,.activity-info strong{display:block;font-size:.84rem;color:#333}
    .approval-info span,.activity-info span{font-size:.75rem;color:#888}.activity-info small{font-size:.7rem;color:#aaa}
    .approval-actions{display:flex;gap:4px}

    .project-item{display:flex;align-items:center;gap:12px;padding:10px 0;border-bottom:1px solid #f9fafb}
    .project-item:last-child{border-bottom:none}
    .project-status{width:8px;height:8px;border-radius:50%;flex-shrink:0}
    .project-info{flex:1}.project-info strong{display:block;font-size:.84rem;color:#333}.project-info span{font-size:.75rem;color:#888}
    .progress-chip{font-size:.68rem!important;height:20px!important}

    .empty-text{text-align:center;color:#aaa;font-size:.82rem;padding:20px 0}

    @media(max-width:1024px){.stats-grid{grid-template-columns:repeat(2,1fr)}.section-grid{grid-template-columns:1fr}.section-card:last-child{grid-column:span 1}}
    @media(max-width:768px){.page{padding:16px}.stats-grid{grid-template-columns:1fr}}
  `]
})
export class DepartmentDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private dashboardSrv = inject(DashboardService);
  private orgSrv = inject(OrganizationService);

  loading = false;
  error: string | null = null;
  orgName = '';
  roleName = '';

  stats = { totalProjects: 0, totalUsers: 0, totalBudget: 0, pendingApprovals: 0 };
  pendingApprovals: any[] = [];
  recentActivity: any[] = [];
  myProjects: any[] = [];

  ngOnInit(): void {
    const user = this.auth.getCurrentUser();
    this.roleName = user?.roles?.join(', ') || 'User';
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.dashboardSrv.getDepartmentDashboard().subscribe({
      next: (r: any) => {
        if (r.success && r.data) {
          this.stats = r.data.stats || this.stats;
          this.pendingApprovals = r.data.pendingApprovals || [];
          this.recentActivity = r.data.recentActivity || [];
          this.myProjects = r.data.myProjects || [];
          this.orgName = r.data.organizationName || 'Department';
        }
        this.loading = false;
        this.error = null;
      },
      error: (e) => { this.loading = false; this.error = e?.error?.message || 'Failed to load dashboard'; }
    });
  }

  approveItem(item: any): void { /* Call approval service */ }
  rejectItem(item: any): void { /* Call approval service */ }

  getPriorityColor(p: string): string {
    const c: any = { High: '#dc2626', Medium: '#f57c00', Low: '#059669' };
    return c[p] || '#888';
  }

  getPriorityIcon(p: string): string {
    const i: any = { High: 'priority_high', Medium: 'warning', Low: 'info' };
    return i[p] || 'help';
  }

  getActivityIcon(type: string): string {
    const i: any = { create: 'add_circle', update: 'edit', delete: 'delete', approve: 'check_circle', login: 'login' };
    return i[type] || 'info';
  }

  getStatusColor(s: string): string {
    const c: any = { Active: '#059669', Planned: '#1976d2', OnHold: '#f57c00', Delayed: '#dc2626', Completed: '#7c3aed' };
    return c[s] || '#888';
  }

  formatTimeAgo(d: string): string {
    if (!d) return '-';
    const diff = Date.now() - new Date(d).getTime();
    const mins = Math.floor(diff / 60000);
    const hrs = Math.floor(mins / 60);
    const days = Math.floor(hrs / 24);
    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    if (hrs < 24) return `${hrs}h ago`;
    return `${days}d ago`;
  }
}