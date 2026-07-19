import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ProfileService } from '../../../core/services/profile.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { HasPermissionDirective } from '../../../core/directives/has-permission.directive';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { DateFormatService } from '../../../core/services/date-format.service';
import { interval, Subscription } from 'rxjs';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-session-management',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule, MatTableModule,
    MatCardModule, MatChipsModule, MatTooltipModule, MatProgressBarModule,
    MatDividerModule, MatFormFieldModule, MatInputModule, HasPermissionDirective,
    LoadingSpinnerComponent, EmptyStateComponent
  ],
  template: `
    <div class="page">
      <div class="header">
        <div>
          <h1>Active Sessions</h1>
          <p>{{ sessions.length }} active sessions across all users</p>
        </div>
        <button mat-stroked-button color="warn" (click)="revokeAllSessions()" 
          *appHasPermission="'UserManagement.Update'" [disabled]="sessions.length === 0">
          <mat-icon>logout</mat-icon> Revoke All Sessions
        </button>
      </div>

      <app-loading-spinner *ngIf="loading"></app-loading-spinner>

      <div class="session-grid" *ngIf="!loading && sessions.length > 0">
        <mat-card *ngFor="let s of sessions" class="session-card" [class.current]="s.isCurrent">
          <mat-card-header>
            <mat-icon mat-card-avatar [style.color]="s.isCurrent ? '#059669' : '#1a73e8'">
              {{ s.isCurrent ? 'person' : 'person_outline' }}
            </mat-icon>
            <mat-card-title>
              {{ s.userName }}
              <mat-chip class="current-chip" *ngIf="s.isCurrent">Current Session</mat-chip>
            </mat-card-title>
            <mat-card-subtitle>{{ s.userEmail }}</mat-card-subtitle>
          </mat-card-header>
          <mat-divider></mat-divider>
          <mat-card-content>
            <div class="session-info">
              <div class="info-row">
                <mat-icon>devices</mat-icon>
                <span>{{ s.deviceInfo || 'Unknown Device' }}</span>
              </div>
              <div class="info-row">
                <mat-icon>language</mat-icon>
                <span>{{ s.ipAddress || 'Unknown IP' }}</span>
              </div>
              <div class="info-row">
                <mat-icon>location_on</mat-icon>
                <span>{{ s.location || 'Unknown Location' }}</span>
              </div>
              <div class="info-row">
                <mat-icon>access_time</mat-icon>
                <span>Active: {{ dateFmt.formatRelative(s.lastActivity) }}</span>
              </div>
              <div class="info-row">
                <mat-icon>timer</mat-icon>
                <span>Duration: {{ getSessionDuration(s) }}</span>
              </div>
            </div>
          </mat-card-content>
          <mat-divider></mat-divider>
          <mat-card-actions>
            <button mat-button color="primary" (click)="viewUserActivity(s)">
              <mat-icon>history</mat-icon> Activity
            </button>
            <ng-container *ngIf="!s.isCurrent">
              <button mat-button color="warn" (click)="revokeSession(s)" 
                *appHasPermission="'UserManagement.Update'">
                <mat-icon>block</mat-icon> Force Logout
              </button>
            </ng-container>
          </mat-card-actions>
        </mat-card>
      </div>

      <app-empty-state
        *ngIf="!loading && sessions.length === 0"
        icon="devices"
        title="No Active Sessions"
        description="All users are currently logged out.">
      </app-empty-state>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1400px;margin:0 auto}
    .header{display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;flex-wrap:wrap;gap:12px}
    .header h1{font-size:1.5rem;font-weight:700;margin:0}.header p{color:#666;font-size:.85rem}
    .session-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(380px,1fr));gap:16px}
    .session-card{border-radius:12px;border:1px solid #e5e7eb;box-shadow:0 1px 3px rgba(0,0,0,.04)}
    .session-card.current{border-color:#a7f3d0;background:#f9fefb}
    .session-card mat-card-header{padding:16px 20px 0}
    .session-card mat-card-content{padding:12px 20px}
    .session-card mat-card-actions{padding:8px 20px 16px;display:flex;gap:8px}
    .current-chip{font-size:.65rem!important;height:20px!important;background:#e6f4ea!important;color:#137333!important}
    .session-info{display:flex;flex-direction:column;gap:6px}
    .info-row{display:flex;align-items:center;gap:8px;font-size:.82rem;color:#555}
    .info-row mat-icon{font-size:16px;width:16px;height:16px;color:#9ca3af}
    @media(max-width:768px){.page{padding:16px}.session-grid{grid-template-columns:1fr}}
  `]
})
export class SessionManagementComponent implements OnInit, OnDestroy {
  private profileSrv = inject(ProfileService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);
  readonly dateFmt = inject(DateFormatService);

  sessions: any[] = [];
  loading = false;
  private refreshSub!: Subscription;

  ngOnInit(): void {
    this.loadSessions();
    this.refreshSub = interval(30000).subscribe(() => this.loadSessions());
  }

  ngOnDestroy(): void { if (this.refreshSub) this.refreshSub.unsubscribe(); }

  loadSessions(): void {
    this.loading = true;
    this.profileSrv.getSessions().subscribe({
      next: (r: any) => {
        if (r.success) this.sessions = r.data || [];
        this.loading = false;
      },
      error: (e: any) => { this.loading = false; }
    });
  }

  getSessionDuration(session: any): string {
    if (!session.loginTime) return '-';
    const start = new Date(session.loginTime).getTime();
    const end = session.isCurrent ? Date.now() : new Date(session.lastActivity).getTime();
    const diff = Math.floor((end - start) / 1000);
    const h = Math.floor(diff / 3600);
    const m = Math.floor((diff % 3600) / 60);
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  }

  async revokeSession(session: any): Promise<void> {
    const result = await Swal.fire({
      title: 'Force Logout?',
      html: `
        <p>Revoke session for <strong>${session.userName}</strong>?</p>
        <p style="color:#666;font-size:.85rem;">Device: ${session.deviceInfo}<br>IP: ${session.ipAddress}</p>
        <p style="color:#dc2626;font-size:.8rem;">User will be logged out immediately.</p>
      `,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      confirmButtonText: 'Yes, force logout',
      cancelButtonText: 'Cancel'
    });
    
    if (!result.isConfirmed) return;

    this.profileSrv.revokeSession(session.id).subscribe({
      next: (r: any) => {
        if (r.success) {
          this.notify.success(`${session.userName} has been logged out`);
          this.loadSessions();
        } else {
          this.notify.error(r.message || 'Failed to revoke session');
        }
      },
      error: (e: any) => this.notify.error('Failed to revoke session')
    });
  }

  async revokeAllSessions(): Promise<void> {
    const result = await Swal.fire({
      title: 'Revoke ALL Sessions?',
      text: 'This will force logout every user except you. This action cannot be undone.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      confirmButtonText: 'Revoke All',
      cancelButtonText: 'Cancel'
    });
    
    if (!result.isConfirmed) return;

    const otherSessions = this.sessions.filter((s: any) => !s.isCurrent);
    let revoked = 0;
    
    for (const s of otherSessions) {
      try {
        await this.profileSrv.revokeSession(s.id).toPromise();
        revoked++;
      } catch {}
    }
    
    this.notify.success(`Revoked ${revoked} sessions`);
    this.loadSessions();
  }

  viewUserActivity(session: any): void {
    window.location.href = `/user-logs?userId=${session.userId}`;
  }
}