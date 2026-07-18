import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { MatMenuModule } from '@angular/material/menu';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { UserLogService, UserLogFilter } from '../../../core/services/user-log.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject, Subscription, interval } from 'rxjs';
import moment from 'moment-timezone';
import * as XLSX from 'xlsx';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-user-log-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule,
    MatNativeDateModule, MatProgressBarModule, MatTooltipModule, MatCardModule,
    MatMenuModule, MatSlideToggleModule, MatDividerModule
  ],
  template: `
    <div class="page">
      <div class="header">
        <div>
          <h1>User Activity Logs</h1>
          <p>{{ totalActiveUsers }} active users • {{ totalFilteredActions }} actions</p>
        </div>
        <div class="header-actions">
          <mat-slide-toggle [(ngModel)]="autoRefresh" (change)="toggleAutoRefresh()" color="primary">Auto-refresh</mat-slide-toggle>
          <button mat-stroked-button color="warn" (click)="clearAllHistory()" *ngIf="auth.hasRole('SuperAdmin')">
            <mat-icon>delete_sweep</mat-icon> Clear History
          </button>
          <button mat-stroked-button [matMenuTriggerFor]="exportMenu"><mat-icon>download</mat-icon> Export</button>
          <mat-menu #exportMenu="matMenu">
            <button mat-menu-item (click)="exportExcel()"><mat-icon style="color:#059669;">table_chart</mat-icon> Excel</button>
            <button mat-menu-item (click)="exportPDF()"><mat-icon style="color:#dc2626;">picture_as_pdf</mat-icon> PDF</button>
          </mat-menu>
        </div>
      </div>

      <div class="stats">
        <div class="stat-card" (click)="openStatsDialog('logins')">
          <div class="stat-icon blue"><mat-icon>login</mat-icon></div>
          <div class="stat-info"><strong>{{ loginCount }}</strong><span>Logins Today</span></div>
        </div>
        <div class="stat-card" (click)="openStatsDialog('active')">
          <div class="stat-icon green"><mat-icon>online_prediction</mat-icon></div>
          <div class="stat-info"><strong>{{ activeCount }}</strong><span>Active Now</span></div>
        </div>
        <div class="stat-card" (click)="openStatsDialog('failed')">
          <div class="stat-icon red"><mat-icon>gpp_bad</mat-icon></div>
          <div class="stat-info"><strong>{{ failedCount }}</strong><span>Failed Logins</span></div>
        </div>
        <div class="stat-card" (click)="openStatsDialog('actions')">
          <div class="stat-icon orange"><mat-icon>receipt_long</mat-icon></div>
          <div class="stat-info"><strong>{{ actionCount }}</strong><span>Actions Today</span></div>
        </div>
      </div>

      <div class="filters">
        <mat-form-field appearance="outline" class="fld">
          <mat-label>Date</mat-label>
          <mat-select [(ngModel)]="dateRange" (selectionChange)="onFilterChange()">
            <mat-option value="today">Today</mat-option>
            <mat-option value="yesterday">Yesterday</mat-option>
            <mat-option value="week">This Week</mat-option>
            <mat-option value="month">This Month</mat-option>
            <mat-option value="all">All Time</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="fld">
          <mat-label>Type</mat-label>
          <mat-select [(ngModel)]="filterType" (selectionChange)="onFilterChange()">
            <mat-option value="">All Types</mat-option>
            <mat-option value="Login">Login</mat-option>
            <mat-option value="Logout">Logout</mat-option>
            <mat-option value="Create">Create</mat-option>
            <mat-option value="Update">Update</mat-option>
            <mat-option value="Delete">Delete</mat-option>
            <mat-option value="View">View</mat-option>
            <mat-option value="FailedLogin">Failed Login</mat-option>
            <mat-option value="Approval">Approval</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="fld">
          <mat-label>Module</mat-label>
          <mat-select [(ngModel)]="filterModule" (selectionChange)="onFilterChange()">
            <mat-option value="">All Modules</mat-option>
            <mat-option *ngFor="let m of activeModules" [value]="m">{{ m }}</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="search">
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="searchText" (ngModelChange)="onSearchChange($event)" placeholder="Search...">
          <button matSuffix mat-icon-button *ngIf="searchText" (click)="clearSearch()"><mat-icon>close</mat-icon></button>
        </mat-form-field>
        <button mat-stroked-button *ngIf="hasFilters" (click)="clearFilters()"><mat-icon>clear_all</mat-icon> Clear</button>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>

      <div class="cards" *ngIf="!loading && userGroups.length > 0">
        <mat-card class="card" *ngFor="let u of userGroups; let i = index">
          <div class="card-hdr" (click)="toggle(i)">
            <div class="av" [style.background]="roleColor(u.userRole)">{{ (u.userName||'?')[0].toUpperCase() }}</div>
            <div class="info">
              <div><strong>{{ u.userName }}</strong><span class="role-tag" [style.background]="roleColor(u.userRole)+'20'" [style.color]="roleColor(u.userRole)">{{ u.userRole }}</span></div>
              <span class="email">{{ u.userEmail }}</span>
              <span class="meta">{{ u.totalActions }} actions</span>
            </div>
            <div class="cnts">
              <span class="cnt c" *ngIf="u.createCount">+{{ u.createCount }}</span>
              <span class="cnt u" *ngIf="u.updateCount">✎{{ u.updateCount }}</span>
              <span class="cnt d" *ngIf="u.deleteCount">🗑{{ u.deleteCount }}</span>
              <span class="cnt v" *ngIf="u.viewCount">👁{{ u.viewCount }}</span>
              <span class="cnt a" *ngIf="u.approvalCount">✓{{ u.approvalCount }}</span>
            </div>
            <mat-icon class="chv" [class.rot]="expanded.has(i)">expand_more</mat-icon>
          </div>
          <div *ngIf="expanded.has(i)">
            <mat-divider></mat-divider>
            <div class="entries">
              <div class="entry" *ngFor="let l of u.logs" (click)="openDetail(l)">
                <span class="badge" [style.background]="actColor(l.activityType)+'18'" [style.color]="actColor(l.activityType)">{{ l.activityType }}</span>
                <span class="mod">{{ l.moduleName }}</span>
                <span class="act">{{ l.action || l.description }}</span>
                <span class="time">{{ fmtTime(l.createdAt) }}</span>
                <mat-icon class="arr">chevron_right</mat-icon>
              </div>
            </div>
          </div>
        </mat-card>
      </div>

      <div class="empty" *ngIf="!loading && userGroups.length === 0">
        <mat-icon>search_off</mat-icon>
        <h3>No Activity Logs</h3>
        <p>User actions will appear here as they interact with business modules.</p>
      </div>
    </div>

    <div class="overlay" *ngIf="dialogOpen" (click)="dialogOpen=false">
      <div class="dlg" (click)="$event.stopPropagation()">
        <div class="dlg-hdr"><h2>{{ dialogTitle }}</h2><button mat-icon-button (click)="dialogOpen=false"><mat-icon>close</mat-icon></button></div>
        <mat-divider></mat-divider>
        <div class="dlg-body">
          <div class="dlg-list" *ngIf="dialogData.length > 0">
            <div class="dlg-item" *ngFor="let item of dialogData" (click)="openDetail(item.logs[0]); dialogOpen=false">
              <mat-icon [style.color]="dialogType==='active'?'#059669':dialogType==='failed'?'#dc2626':'#2563eb'">{{ dialogType==='active'?'online_prediction':dialogType==='failed'?'error':'person' }}</mat-icon>
              <div class="dlg-info">
                <strong>{{ item.userName || 'Unknown' }}</strong>
                <span>{{ item.userEmail }} • {{ item.userRole }}</span>
                <span *ngIf="dialogType==='logins'">Logged in {{ item.count }} time(s)</span>
                <span *ngIf="dialogType==='active'" class="onln">● Online now</span>
                <span *ngIf="dialogType==='failed'" class="red">Attempted: {{ item.attemptedEmail }}</span>
                <span *ngIf="dialogType==='actions'">
                  <span class="mc c" *ngIf="item.createCount">+{{ item.createCount }}</span>
                  <span class="mc u" *ngIf="item.updateCount">✎{{ item.updateCount }}</span>
                  <span class="mc d" *ngIf="item.deleteCount">🗑{{ item.deleteCount }}</span>
                  <span class="mc v" *ngIf="item.viewCount">👁{{ item.viewCount }}</span>
                  <span class="mc a" *ngIf="item.approvalCount">✓{{ item.approvalCount }}</span>
                </span>
                <small>{{ fmtTime(item.lastActivity) }}</small>
              </div>
              <mat-icon class="arr">chevron_right</mat-icon>
            </div>
          </div>
          <div class="empty" *ngIf="dialogData.length===0"><p>No records</p></div>
        </div>
      </div>
    </div>

    <div class="overlay" *ngIf="detailLog" (click)="detailLog=null">
      <div class="panel" (click)="$event.stopPropagation()">
        <div class="dlg-hdr"><h2>Request Details</h2><button mat-icon-button (click)="detailLog=null"><mat-icon>close</mat-icon></button></div>
        <mat-divider></mat-divider>
        <div class="pnl-body" *ngIf="detailData">
          <mat-progress-bar *ngIf="detailLoading" mode="indeterminate" color="primary"></mat-progress-bar>
          <ng-container *ngIf="!detailLoading">
            <div class="sec"><h3><mat-icon>person</mat-icon> User</h3>
              <div class="row"><span>Name</span><strong>{{ detailData.log?.userName }}</strong></div>
              <div class="row"><span>Email</span><strong>{{ detailData.log?.userEmail }}</strong></div>
              <div class="row"><span>Role</span><strong>{{ detailData.log?.userRole }}</strong></div>
            </div>
            <mat-divider></mat-divider>
            <div class="sec"><h3><mat-icon>info</mat-icon> Activity</h3>
              <div class="row"><span>Type</span><strong>{{ detailData.log?.activityType }}</strong></div>
              <div class="row"><span>Module</span><strong>{{ detailData.log?.moduleName }}</strong></div>
              <div class="row"><span>Action</span><strong>{{ detailData.log?.action || detailData.log?.description }}</strong></div>
            </div>
            <mat-divider></mat-divider>
            <div class="sec"><h3><mat-icon>dns</mat-icon> Session</h3>
              <div class="row"><span>IP</span><strong>{{ detailData.log?.ipAddress }}</strong></div>
              <div class="row"><span>Device</span><strong>{{ detailData.log?.deviceInfo }}</strong></div>
            </div>
            <mat-divider></mat-divider>
            <div class="sec"><h3><mat-icon>http</mat-icon> Request</h3>
              <div class="row"><span>Method</span><strong>{{ detailData.log?.requestMethod }}</strong></div>
              <div class="row"><span>URL</span><strong class="url">{{ detailData.log?.requestUrl }}</strong></div>
              <div class="row"><span>Status</span><strong>{{ detailData.log?.responseStatus }}</strong></div>
              <div class="row"><span>Time</span><strong>{{ fmtTime(detailData.log?.createdAt) }}</strong></div>
            </div>
            <mat-divider></mat-divider>
            
            <!-- Session History Section -->
            <div class="sec">
              <h3>
                <mat-icon>history</mat-icon> Session History
                <span class="toggle-session" (click)="toggleSessionHistory()">
                  <mat-icon>{{ showSessionHistory ? 'expand_less' : 'expand_more' }}</mat-icon>
                </span>
              </h3>
              <div class="session-history" *ngIf="showSessionHistory">
                <mat-progress-bar *ngIf="sessionLoading" mode="indeterminate" color="primary"></mat-progress-bar>
                <div *ngIf="!sessionLoading && userSessions.length > 0" class="session-list">
                  <div class="session-item" *ngFor="let session of userSessions" [class.login]="session.activityType === 'Login'" [class.logout]="session.activityType === 'Logout'">
                    <div class="session-left">
                      <mat-icon class="session-icon" [style.color]="session.activityType === 'Login' ? '#059669' : '#dc2626'">
                        {{ session.activityType === 'Login' ? 'login' : 'logout' }}
                      </mat-icon>
                      <div class="session-details">
                        <span class="session-type">{{ session.activityType }}</span>
                        <span class="session-time-nepali">{{ toNepaliDateTime(session.createdAt) }}</span>
                        <span class="session-time-english">{{ fmtTime(session.createdAt) }}</span>
                      </div>
                    </div>
                    <div class="session-right">
                      <span class="session-ip">{{ session.ipAddress || 'N/A' }}</span>
                      <span class="session-device">{{ session.deviceInfo || 'Unknown Device' }}</span>
                    </div>
                  </div>
                </div>
                <div class="no-sessions" *ngIf="!sessionLoading && userSessions.length === 0">
                  <p>No login/logout history available</p>
                </div>
              </div>
            </div>
            <mat-divider></mat-divider>
            
            <div class="sec" *ngIf="detailData.log?.requestBody">
              <h3><mat-icon>code</mat-icon> {{ detailData.log?.activityType==='FailedLogin'?'Failed Login':detailData.log?.activityType==='Approval'?'Approval Details':'Request Body' }}</h3>
              <div class="fail" *ngIf="detailData.log?.activityType==='FailedLogin'">
                <p><strong>Email:</strong> {{ extractEmail(detailData.log?.requestBody) }}</p>
                <p><strong>Password:</strong> {{ extractPass(detailData.log?.requestBody) }}</p>
                <p><strong>Reason:</strong> Invalid credentials</p>
              </div>
              <div class="approval" *ngIf="detailData.log?.activityType==='Approval'">
                <p><strong>Status:</strong> {{ extractApprovalStatus(detailData.log?.requestBody) }}</p>
                <p><strong>Comment:</strong> {{ extractApprovalComment(detailData.log?.requestBody) }}</p>
              </div>
              <pre class="code" *ngIf="detailData.log?.activityType!=='FailedLogin' && detailData.log?.activityType!=='Approval'">{{ fmtJson(detailData.log?.requestBody) }}</pre>
            </div>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1440px;margin:0 auto}
    .header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:20px;gap:12px;flex-wrap:wrap}
    .header h1{font-size:1.5rem;font-weight:700;margin:0;color:#111}
    .header p{color:#888;font-size:.85rem;margin:2px 0 0}
    .header-actions{display:flex;align-items:center;gap:8px;flex-wrap:wrap}
    .stats{display:grid;grid-template-columns:repeat(4,1fr);gap:14px;margin-bottom:18px}
    .stat-card{display:flex;align-items:center;gap:12px;background:#fff;padding:16px 18px;border-radius:12px;cursor:pointer;border:1px solid #eee;transition:all .2s}
    .stat-card:hover{transform:translateY(-2px);box-shadow:0 4px 16px rgba(0,0,0,.08)}
    .stat-icon{width:42px;height:42px;border-radius:10px;display:flex;align-items:center;justify-content:center}
    .stat-icon mat-icon{font-size:22px;width:22px;height:22px}
    .blue{background:#eff6ff;color:#2563eb}.green{background:#ecfdf5;color:#059669}.red{background:#fef2f2;color:#dc2626}.orange{background:#fffbeb;color:#d97706}
    .stat-info strong{display:block;font-size:1.4rem;color:#111}.stat-info span{font-size:.78rem;color:#888}
    .filters{display:flex;gap:10px;flex-wrap:wrap;align-items:center;margin-bottom:16px;padding:12px 16px;background:#fff;border-radius:12px;border:1px solid #eee}
    .fld{width:140px}.search{width:260px;flex:1;min-width:180px}
    mat-form-field{margin-bottom:-6px}
    .cards{display:flex;flex-direction:column;gap:8px}
    .card{border-radius:12px;border:1px solid #eee;box-shadow:none}
    .card-hdr{display:flex;align-items:center;gap:12px;padding:14px 18px;cursor:pointer}
    .card-hdr:hover{background:#fafafa}
    .av{width:40px;height:40px;border-radius:50%;display:flex;align-items:center;justify-content:center;color:#fff;font-weight:700;font-size:1rem;flex-shrink:0}
    .info{flex:1;min-width:0}
    .info strong{font-size:.9rem}.email{font-size:.78rem;color:#888;display:block}.meta{font-size:.73rem;color:#aaa}
    .role-tag{padding:2px 8px;border-radius:10px;font-size:.68rem;font-weight:600;margin-left:6px}
    .cnts{display:flex;gap:4px}.cnt{font-size:.7rem;padding:2px 7px;border-radius:10px;font-weight:600}
    .c{background:#eff6ff;color:#2563eb}.u{background:#fffbeb;color:#d97706}.d{background:#fef2f2;color:#dc2626}.v{background:#ecfdf5;color:#059669}.a{background:#f3e8ff;color:#7c3aed}
    .chv{color:#ccc;transition:transform .3s}.rot{transform:rotate(180deg)}
    .entries{max-height:380px;overflow-y:auto}
    .entry{display:flex;align-items:center;gap:10px;padding:10px 18px;cursor:pointer;border-bottom:1px solid #f5f5f5;font-size:.83rem}
    .entry:hover{background:#f8faff}
    .badge{padding:2px 8px;border-radius:10px;font-size:.7rem;font-weight:600;white-space:nowrap}
    .mod{font-weight:500;color:#333;min-width:100px}.act{flex:1;color:#666;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
    .time{font-size:.73rem;color:#999;white-space:nowrap}.arr{color:#ccc;font-size:18px;width:18px;height:18px}
    .empty{text-align:center;padding:80px 20px;color:#aaa}.empty mat-icon{font-size:56px;width:56px;height:56px;color:#ddd}
    .overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1000;display:flex;justify-content:center;align-items:center}
    .dlg{width:550px;max-width:93vw;max-height:75vh;background:#fff;border-radius:16px;overflow-y:auto;box-shadow:0 20px 60px rgba(0,0,0,.3)}
    .panel{width:520px;max-width:93vw;background:#fff;height:100%;overflow-y:auto;box-shadow:-8px 0 35px rgba(0,0,0,.2);position:fixed;right:0}
    .dlg-hdr{display:flex;justify-content:space-between;align-items:center;padding:16px 20px;position:sticky;top:0;background:#fff;z-index:5}
    .dlg-hdr h2{font-size:1.1rem;font-weight:600;margin:0}
    .dlg-body,.pnl-body{padding:14px 20px 24px}
    .dlg-list{display:flex;flex-direction:column;gap:4px}
    .dlg-item{display:flex;align-items:center;gap:10px;padding:12px;border-radius:10px;cursor:pointer}
    .dlg-item:hover{background:#f5f7ff}
    .dlg-info{flex:1;display:flex;flex-direction:column;gap:2px}
    .dlg-info strong{font-size:.88rem}.dlg-info span{font-size:.78rem;color:#888}.dlg-info small{font-size:.7rem;color:#bbb}
    .onln{color:#059669!important;font-weight:600}.red{color:#dc2626!important;font-weight:500}
    .mc{font-size:.65rem;padding:1px 5px;border-radius:8px;font-weight:600;margin-right:2px}
    .sec{margin-bottom:16px}.sec h3{display:flex;align-items:center;gap:8px;font-size:.8rem;font-weight:700;color:#2563eb;text-transform:uppercase;margin:0 0 8px}
    .sec h3 mat-icon{font-size:17px;width:17px;height:17px}
    .row{display:flex;justify-content:space-between;padding:4px 0;font-size:.83rem}
    .row span{color:#888}.row strong{color:#333;text-align:right;max-width:300px;word-break:break-all}
    .url{font-size:.72rem!important;color:#2563eb!important;font-family:monospace}
    .code{background:#1e1e2e;color:#cdd6f4;border-radius:8px;padding:14px;font-size:.76rem;font-family:monospace;overflow-x:auto;max-height:300px;white-space:pre-wrap;margin:0}
    .fail{background:#fffbeb;border-radius:8px;padding:12px;border-left:4px solid #f59e0b}
    .fail p{margin:4px 0;font-size:.85rem}
    .approval{background:#f3e8ff;border-radius:8px;padding:12px;border-left:4px solid #7c3aed}
    .approval p{margin:4px 0;font-size:.85rem}
    
    /* Session History Styles */
    .toggle-session {
      margin-left: auto;
      cursor: pointer;
      display: flex;
      align-items: center;
      padding: 4px;
      border-radius: 4px;
      transition: background 0.2s;
    }
    .toggle-session:hover {
      background: rgba(37, 99, 235, 0.1);
    }
    .session-history {
      margin-top: 8px;
    }
    .session-list {
      max-height: 400px;
      overflow-y: auto;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
    }
    .session-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
      border-bottom: 1px solid #f3f4f6;
      transition: background 0.2s;
    }
    .session-item:last-child {
      border-bottom: none;
    }
    .session-item:hover {
      background: #f9fafb;
    }
    .session-item.login {
      border-left: 3px solid #059669;
    }
    .session-item.logout {
      border-left: 3px solid #dc2626;
    }
    .session-left {
      display: flex;
      align-items: center;
      gap: 12px;
      flex: 1;
    }
    .session-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }
    .session-details {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    .session-type {
      font-size: 0.85rem;
      font-weight: 600;
      color: #374151;
    }
    .session-time-nepali {
      font-size: 0.78rem;
      color: #6b7280;
      font-family: 'Mukta', 'Noto Sans', sans-serif;
    }
    .session-time-english {
      font-size: 0.72rem;
      color: #9ca3af;
    }
    .session-right {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 2px;
    }
    .session-ip {
      font-size: 0.75rem;
      color: #6b7280;
      font-family: monospace;
    }
    .session-device {
      font-size: 0.72rem;
      color: #9ca3af;
      max-width: 200px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .no-sessions {
      padding: 24px;
      text-align: center;
      color: #9ca3af;
      background: #f9fafb;
      border-radius: 8px;
      border: 1px solid #e5e7eb;
    }
    .no-sessions p {
      margin: 0;
      font-size: 0.85rem;
    }
    
    @media(max-width:768px){.page{padding:14px}.stats{grid-template-columns:repeat(2,1fr)}.panel{width:100vw}.dlg{width:95vw}.cnts{display:none}}
  `]
})
export class UserLogListComponent implements OnInit, OnDestroy {
  private srv = inject(UserLogService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);

  allRawLogs: any[] = [];
  userGroups: any[] = [];
  activeModules: string[] = [];
  expanded = new Set<number>();
  loading = false;

  dateRange = 'today';
  filterType = '';
  filterModule = '';
  searchText = '';
  private search$ = new Subject<string>();
  private searchSubscription!: Subscription;
  autoRefresh = false;
  private refreshSub!: Subscription;

  dialogOpen = false;
  dialogTitle = '';
  dialogType = '';
  dialogData: any[] = [];

  detailLog: any = null;
  detailData: any = null;
  detailLoading = false;

  // Session history
  showSessionHistory = false;
  sessionLoading = false;
  userSessions: any[] = [];

  // FIX: Only exclude Dashboard and UserLogs, allow everything else including Auth and ApprovalWorkflow
  private readonly EXCLUDE = ['Dashboard', 'UserLogs'];

  get businessLogs(): any[] {
    const seen = new Set<string>();
    return this.allRawLogs.filter(l => {
      // Skip OPTIONS requests
      if (l.requestMethod === 'OPTIONS') return false;
      // Skip system/anonymous users
      if (!l.userName || l.userName === 'System' || l.userName === 'Anonymous') return false;
      // Admin visibility restriction
      if (this.auth.hasRole('Admin') && !this.auth.hasRole('SuperAdmin') && l.userRole === 'SuperAdmin') return false;
      // Skip excluded modules
      if (this.EXCLUDE.includes(l.moduleName)) return false;
      // Remove duplicates
      if (seen.has(l.id)) return false;
      seen.add(l.id);
      return true;
    });
  }

  get totalActiveUsers(): number {
    const users = new Set(
      this.allRawLogs
        .filter(l => l.userName && l.userName !== 'System' && l.userName !== 'Anonymous')
        .map(l => l.userId || l.userEmail)
    );
    return users.size;
  }

  get totalFilteredActions(): number { return this.businessLogs.length; }

  get loginCount(): number {
    const today = moment().startOf('day').toISOString();
    
    const directLogins = this.allRawLogs.filter(l => 
      l.activityType === 'Login' && 
      l.createdAt >= today && 
      l.userName && 
      l.userName !== 'System' && 
      l.userName !== 'Anonymous'
    );
    
    if (directLogins.length > 0) {
      const users = new Set(directLogins.map(l => l.userId || l.userEmail));
      return users.size;
    }
    
    const todayUsers = new Map<string, any[]>();
    
    this.allRawLogs
      .filter(l => 
        l.createdAt >= today && 
        l.userName && 
        l.userName !== 'System' && 
        l.userName !== 'Anonymous'
      )
      .forEach(l => {
        const key = l.userId || l.userEmail;
        if (!todayUsers.has(key)) {
          todayUsers.set(key, []);
        }
        todayUsers.get(key)!.push(l);
      });
    
    let inferredLogins = 0;
    
    todayUsers.forEach((logs, key) => {
      const hasLogout = logs.some(l => l.activityType === 'Logout');
      
      const hasAuthActivity = logs.some(l => 
        l.moduleName === 'Auth' && 
        (l.activityType === 'Logout' || l.action?.toLowerCase().includes('logged in') || l.description?.toLowerCase().includes('logged in'))
      );
      
      if (hasLogout || hasAuthActivity) {
        inferredLogins++;
      } else if (logs.length >= 2) {
        inferredLogins++;
      }
    });
    
    return inferredLogins;
  }

  get activeCount(): number {
    const cutoff = moment().subtract(15, 'minutes').toISOString();
    const users = new Set(
      this.allRawLogs
        .filter(l => l.createdAt >= cutoff && l.userName && l.userName !== 'System' && l.userName !== 'Anonymous')
        .map(l => l.userId || l.userEmail)
    );
    return users.size;
  }

  get failedCount(): number {
    const today = moment().startOf('day').toISOString();
    const users = new Set(
      this.allRawLogs
        .filter(l => l.activityType === 'FailedLogin' && l.createdAt >= today && l.userName && l.userName !== 'System' && l.userName !== 'Anonymous')
        .map(l => l.userId || l.userEmail || l.ipAddress)
    );
    return users.size;
  }

  get actionCount(): number {
    const today = moment().startOf('day').toISOString();
    return this.businessLogs.filter(l =>
      l.createdAt >= today &&
      l.activityType !== 'Login' &&
      l.activityType !== 'Logout' &&
      l.activityType !== 'FailedLogin'
    ).length;
  }

  get hasFilters(): boolean { return !!(this.filterType || this.filterModule || this.searchText || this.dateRange !== 'today'); }

  ngOnInit(): void {
    this.searchSubscription = this.search$.pipe(debounceTime(400)).subscribe(() => {
      this.loadData();
    });
    this.loadData();
  }
  
  ngOnDestroy(): void {
    if (this.refreshSub) this.refreshSub.unsubscribe();
    if (this.searchSubscription) this.searchSubscription.unsubscribe();
  }

  onFilterChange(): void {
    this.loadData();
  }

  onSearchChange(value: string): void {
    this.searchText = value;
    this.search$.next(value);
  }

  clearSearch(): void {
    this.searchText = '';
    this.search$.next('');
  }

  loadData(): void {
    this.loading = true;
    const p: UserLogFilter = {
      page: 1,
      pageSize: 2000,
      sortBy: 'createdAt',
      sortOrder: 'desc',
      search: this.searchText || undefined,
      activityType: this.filterType || undefined,
      moduleName: this.filterModule || undefined
    };
    
    const now = moment();
    switch (this.dateRange) {
      case 'today':
        p.startDate = now.startOf('day').toISOString();
        break;
      case 'yesterday':
        const yesterdayStart = moment().subtract(1, 'day').startOf('day');
        const yesterdayEnd = moment().startOf('day');
        p.startDate = yesterdayStart.toISOString();
        p.endDate = yesterdayEnd.toISOString();
        break;
      case 'week':
        p.startDate = now.startOf('week').toISOString();
        break;
      case 'month':
        p.startDate = now.startOf('month').toISOString();
        break;
      case 'all':
        // Don't set any date filter for all time
        break;
    }

    this.srv.getLogs(p).subscribe({
      next: (r: any) => {
        if (r.success) {
          this.allRawLogs = r.data || [];
          this.buildUsers();
          this.buildModules();
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  buildUsers(): void {
    const map = new Map<string, any>();
    this.businessLogs.forEach(l => {
      const key = l.userId || l.userEmail || l.userName;
      if (!map.has(key)) {
        map.set(key, {
          userId: l.userId,
          userName: l.userName,
          userEmail: l.userEmail,
          userRole: l.userRole,
          totalActions: 0,
          createCount: 0,
          updateCount: 0,
          deleteCount: 0,
          viewCount: 0,
          approvalCount: 0,
          lastActivity: l.createdAt,
          logs: []
        });
      }
      const u = map.get(key);
      u.totalActions++;
      u.logs.push(l);
      if (l.activityType === 'Create') u.createCount++;
      else if (l.activityType === 'Update') u.updateCount++;
      else if (l.activityType === 'Delete') u.deleteCount++;
      else if (l.activityType === 'View') u.viewCount++;
      else if (l.activityType === 'Approval') u.approvalCount++;
      if (l.createdAt > u.lastActivity) u.lastActivity = l.createdAt;
    });
    this.userGroups = Array.from(map.values()).sort((a, b) => b.totalActions - a.totalActions);
  }

  buildModules(): void {
    const set = new Set<string>();
    this.allRawLogs.forEach(l => { 
      if (l.moduleName && !this.EXCLUDE.includes(l.moduleName)) {
        set.add(l.moduleName); 
      }
    });
    this.activeModules = Array.from(set).sort();
  }

  toggleAutoRefresh(): void {
    if (this.autoRefresh) {
      this.refreshSub = interval(30000).subscribe(() => this.loadData());
    } else {
      if (this.refreshSub) this.refreshSub.unsubscribe();
    }
  }

  clearFilters(): void {
    this.filterType = '';
    this.filterModule = '';
    this.searchText = '';
    this.dateRange = 'today';
    this.loadData();
  }
  
  toggle(i: number): void {
    if (this.expanded.has(i)) {
      this.expanded.delete(i);
    } else {
      this.expanded.add(i);
    }
  }

  toggleSessionHistory(): void {
    this.showSessionHistory = !this.showSessionHistory;
    if (this.showSessionHistory && this.detailData?.log) {
      this.loadUserSessions();
    }
  }

  loadUserSessions(): void {
    if (!this.detailData?.log) return;
    
    this.sessionLoading = true;
    const userEmail = this.detailData.log.userEmail;
    const userId = this.detailData.log.userId;

    // Fetch ALL logs without filters to get complete session history
    const filter: UserLogFilter = {
      page: 1,
      pageSize: 5000,
      sortBy: 'createdAt',
      sortOrder: 'desc'
      // Don't set any filters to get all data
    };

    this.srv.getLogs(filter).subscribe({
      next: (r: any) => {
        if (r.success) {
          // Filter only Login and Logout activities for this user from all records
          this.userSessions = (r.data || []).filter((l: any) => 
            (l.activityType === 'Login' || l.activityType === 'Logout') &&
            (l.userEmail === userEmail || l.userId === userId || l.userName === this.detailData.log.userName)
          ).sort((a: any, b: any) => 
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          );
          
          // If no sessions found, try alternate matching
          if (this.userSessions.length === 0) {
            this.userSessions = (r.data || []).filter((l: any) => 
              (l.activityType === 'Login' || l.activityType === 'Logout') &&
              (l.userEmail?.toLowerCase() === userEmail?.toLowerCase() || 
               l.userId === userId ||
               l.userName?.toLowerCase() === this.detailData.log.userName?.toLowerCase())
            ).sort((a: any, b: any) => 
              new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
            );
          }
        }
        this.sessionLoading = false;
      },
      error: () => {
        this.sessionLoading = false;
      }
    });
  }

  // Convert to Nepali date and time
  toNepaliDateTime(isoString: string): string {
    if (!isoString) return '-';
    
    try {
      const date = new Date(isoString);
      
      const nepaliMonths = [
        'बैशाख', 'जेठ', 'असार', 'साउन', 'भदौ', 'असोज', 
        'कार्तिक', 'मंसिर', 'पुष', 'माघ', 'फागुन', 'चैत'
      ];
      
      const nepaliDays = [
        'आइतबार', 'सोमबार', 'मंगलबार', 'बुधबार', 
        'बिहिबार', 'शुक्रबार', 'शनिबार'
      ];
      
      const nepaliNumbers = ['०', '१', '२', '३', '४', '५', '६', '७', '८', '९'];
      
      const toNepaliNum = (num: number): string => {
        return num.toString().split('').map(d => nepaliNumbers[parseInt(d)] || d).join('');
      };
      
      const dayOfWeek = nepaliDays[date.getDay()];
      const day = date.getDate();
      const monthIndex = date.getMonth();
      const year = date.getFullYear();
      const nepaliYear = year + 56;
      
      const hours = date.getHours();
      const minutes = date.getMinutes();
      const hours12 = hours % 12 || 12;
      
      return `${dayOfWeek}, ${nepaliMonths[monthIndex]} ${toNepaliNum(day)}, ${toNepaliNum(nepaliYear)} • ${toNepaliNum(hours12)}:${toNepaliNum(minutes).toString().padStart(2, '०')}`;
      
    } catch (e) {
      return isoString;
    }
  }

  openStatsDialog(type: string): void {
    this.dialogType = type;
    this.dialogOpen = true;
    const today = moment().startOf('day').toISOString();
    const fifteenMin = moment().subtract(15, 'minutes').toISOString();

    let filtered: any[] = [];

    switch (type) {
      case 'logins':
        this.dialogTitle = 'Logins Today';
        filtered = this.allRawLogs.filter(l =>
          l.activityType === 'Login' && l.createdAt >= today
          && l.userName && l.userName !== 'System' && l.userName !== 'Anonymous'
        );
        
        if (filtered.length === 0) {
          const todayLogs = this.allRawLogs.filter(l =>
            l.createdAt >= today && 
            l.userName && 
            l.userName !== 'System' && 
            l.userName !== 'Anonymous'
          );
          
          const userMap = new Map<string, any[]>();
          todayLogs.forEach(l => {
            const key = l.userId || l.userEmail;
            if (!userMap.has(key)) userMap.set(key, []);
            userMap.get(key)!.push(l);
          });
          
          userMap.forEach((logs, key) => {
            const hasLogout = logs.some(l => l.activityType === 'Logout');
            const hasAuthActivity = logs.some(l => 
              l.moduleName === 'Auth' && 
              (l.activityType === 'Logout' || l.action?.toLowerCase().includes('logged in'))
            );
            
            if (hasLogout || hasAuthActivity) {
              const firstLog = logs.reduce((earliest, current) => 
                current.createdAt < earliest.createdAt ? current : earliest
              );
              filtered.push({
                ...firstLog,
                activityType: 'Login',
                moduleName: 'Auth',
                action: 'User logged in (inferred from session)',
                description: 'Login detected from session activity pattern'
              });
            } else if (logs.length >= 2) {
              const firstLog = logs.reduce((earliest, current) => 
                current.createdAt < earliest.createdAt ? current : earliest
              );
              filtered.push({
                ...firstLog,
                activityType: 'Login',
                moduleName: 'Auth',
                action: 'User logged in (inferred from new session)',
                description: 'Login detected from new session pattern'
              });
            }
          });
        }
        break;
      case 'active':
        this.dialogTitle = 'Active Users Now';
        filtered = this.allRawLogs.filter(l =>
          l.createdAt >= fifteenMin && l.userName && l.userName !== 'System' && l.userName !== 'Anonymous'
        );
        break;
      case 'failed':
        this.dialogTitle = 'Failed Login Attempts';
        filtered = this.allRawLogs.filter(l =>
          l.activityType === 'FailedLogin' && l.createdAt >= today
          && l.userName && l.userName !== 'System' && l.userName !== 'Anonymous'
        );
        break;
      case 'actions':
        this.dialogTitle = 'Actions Today';
        filtered = this.businessLogs.filter(l =>
          l.createdAt >= today && l.activityType !== 'Login' && l.activityType !== 'Logout' && l.activityType !== 'FailedLogin'
        );
        break;
    }

    this.dialogData = this.groupDialog(filtered, type);
  }

  groupDialog(data: any[], type: string): any[] {
    const map = new Map<string, any>();
    data.forEach(d => {
      const key = d.userId || d.userEmail || d.userName || d.ipAddress || 'Unknown';
      if (!map.has(key)) {
        map.set(key, {
          userName: d.userName,
          userEmail: d.userEmail,
          userRole: d.userRole,
          count: 1,
          lastActivity: d.createdAt,
          attemptedEmail: type === 'failed' ? this.extractEmail(d.requestBody) : undefined,
          createCount: 0,
          updateCount: 0,
          deleteCount: 0,
          viewCount: 0,
          approvalCount: 0,
          logs: [d]
        });
      } else {
        const item = map.get(key);
        item.count++;
        item.logs.push(d);
        if (d.createdAt > item.lastActivity) item.lastActivity = d.createdAt;
      }
      if (type === 'actions') {
        const item = map.get(key);
        if (d.activityType === 'Create') item.createCount++;
        else if (d.activityType === 'Update') item.updateCount++;
        else if (d.activityType === 'Delete') item.deleteCount++;
        else if (d.activityType === 'View') item.viewCount++;
        else if (d.activityType === 'Approval') item.approvalCount++;
      }
    });
    return Array.from(map.values()).sort((a, b) => b.count - a.count);
  }

  openDetail(log: any): void {
    this.detailLog = log;
    this.detailData = null;
    this.detailLoading = true;
    this.showSessionHistory = true; // Auto-expand session history
    this.userSessions = [];
    
    this.srv.getLogDetail(log.id).subscribe({
      next: (r: any) => {
        if (r.success) {
          this.detailData = r.data;
          // Load session history immediately
          this.loadUserSessions();
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
      }
    });
  }

  async clearAllHistory(): Promise<void> {
    const result = await Swal.fire({
      title: '⚠️ Clear All Activity History?',
      html: `
        <div style="text-align:left;padding:8px 0;">
          <p style="margin:0 0 12px;color:#374151;font-size:0.9rem;">You are about to <strong style="color:#dc2626;">permanently delete</strong> every activity record in the system.</p>
          <div style="background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:12px;margin-bottom:8px;">
            <p style="margin:0;color:#991b1b;font-size:0.82rem;font-weight:600;">🔴 This action is <u>permanent</u> and cannot be undone.</p>
          </div>
          <p style="margin:0;color:#6b7280;font-size:0.8rem;">All login records, CRUD operations, and audit trails will be erased.</p>
        </div>
      `,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      confirmButtonText: 'Yes, delete everything',
      cancelButtonText: 'No, keep my data',
      reverseButtons: true
    });
    if (!result.isConfirmed) return;

    const confirmResult = await Swal.fire({
      title: 'Final Confirmation Required',
      html: `
        <div style="text-align:center;">
          <p style="margin:0 0 14px;color:#374151;font-size:0.9rem;">To verify, type <strong style="color:#dc2626;font-size:1.1rem;">DELETE</strong> in the field below:</p>
          <p style="margin:0;color:#9ca3af;font-size:0.75rem;">This ensures you understand the consequences.</p>
        </div>
      `,
      input: 'text',
      inputPlaceholder: 'Type DELETE here',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      confirmButtonText: 'Permanently Delete All',
      cancelButtonText: 'Cancel',
      inputValidator: (v) => {
        if (!v) return 'Please type DELETE to confirm';
        if (v !== 'DELETE') return 'You must type DELETE exactly as shown';
        return null;
      }
    });
    if (!confirmResult.isConfirmed || confirmResult.value !== 'DELETE') return;

    this.srv.purgeLogs(0).subscribe({
      next: (r: any) => {
        if (r.success) {
          this.notify.success(`All history cleared. ${r.data?.purgedCount || 0} records removed.`);
          this.allRawLogs = [];
          this.userGroups = [];
          this.activeModules = [];
          this.expanded.clear();
          this.dialogOpen = false;
          this.detailLog = null;
          this.detailData = null;
        } else {
          this.notify.error('Purge failed on backend.');
        }
      },
      error: () => this.notify.error('Backend purge endpoint failed.')
    });
  }

  exportExcel(): void {
    const data = this.businessLogs.map(r => ({
      User: r.userName,
      Email: r.userEmail,
      Activity: r.activityType,
      Module: r.moduleName,
      Action: r.action || r.description,
      Method: r.requestMethod,
      IP: r.ipAddress,
      Status: r.responseStatus,
      Time: this.fmtTime(r.createdAt)
    }));
    const ws = XLSX.utils.json_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Activity');
    XLSX.writeFile(wb, `activity_${moment().format('YYYYMMDD_HHmmss')}.xlsx`);
    this.notify.success('Exported!');
  }

  exportPDF(): void {
    const doc = new jsPDF('landscape');
    doc.setFontSize(14);
    doc.text('Activity Logs', 14, 15);
    const rows = this.businessLogs.map(r => [
      r.userName,
      r.activityType,
      r.moduleName,
      (r.action || r.description || '').substring(0, 50),
      r.requestMethod,
      r.ipAddress,
      this.fmtTime(r.createdAt)
    ]);
    autoTable(doc, {
      head: [['User', 'Activity', 'Module', 'Action', 'Method', 'IP', 'Time']],
      body: rows,
      startY: 22,
      styles: { fontSize: 7 },
      headStyles: { fillColor: [37, 99, 235] }
    });
    doc.save(`activity_${moment().format('YYYYMMDD_HHmmss')}.pdf`);
    this.notify.success('Exported!');
  }

  fmtTime(d: string): string {
    if (!d) return '-';
    return moment(d).tz ? moment(d).tz('Asia/Kathmandu').format('ddd, DD MMM YYYY, hh:mm A') : moment(d).format('DD/MM/YYYY HH:mm');
  }
  
  fmtJson(j: string): string {
    try { return JSON.stringify(JSON.parse(j), null, 2); } catch { return j || '-'; }
  }
  
  extractEmail(b: string): string {
    if (!b) return 'Unknown';
    try { return JSON.parse(b).email || 'Unknown'; } catch { return b.substring(0, 50); }
  }
  
  extractPass(b: string): string {
    if (!b) return 'N/A';
    try { return JSON.parse(b).password ? '••••••••' : 'N/A'; } catch { return 'N/A'; }
  }
  
  extractApprovalStatus(b: string): string { 
    if (!b) return 'N/A'; 
    try { 
      const data = JSON.parse(b);
      return data.status || data.approvalStatus || 'Pending';
    } catch { 
      return b.includes('approved') ? 'Approved' : b.includes('rejected') ? 'Rejected' : 'Pending';
    }
  }
  
  extractApprovalComment(b: string): string {
    if (!b) return 'No comment';
    try {
      const data = JSON.parse(b);
      return data.comment || data.remarks || data.note || 'No comment';
    } catch {
      return b.substring(0, 100) || 'No comment';
    }
  }
  
  actColor(t: string): string { 
    const m: any = { 
      Login: '#059669', 
      Logout: '#6b7280', 
      Create: '#2563eb', 
      Update: '#d97706', 
      Delete: '#dc2626', 
      View: '#0891b2', 
      FailedLogin: '#dc2626', 
      AccessDenied: '#ea580c', 
      Approval: '#7c3aed' 
    }; 
    return m[t] || '#6b7280'; 
  }
  
  roleColor(r: string): string { 
    const m: any = { 
      SuperAdmin: '#7c3aed', 
      Admin: '#2563eb', 
      Test: '#059669', 
      Viewer: '#6b7280' 
    }; 
    return m[r] || '#6b7280'; 
  }
}