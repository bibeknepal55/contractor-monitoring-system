import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { ExecutiveDashboard } from '../../core/models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatButtonModule,
    NgxChartsModule,
    PageHeaderComponent,
    StatCardComponent,
  ],
  template: `
    <!-- ========== VIEWER ACCESS DENIED PAGE ========== -->
    <div class="viewer-page" *ngIf="isViewerOnly">
      <div class="viewer-container">
        <div class="viewer-card">
          <div class="shield-wrapper">
            <div class="shield-outer">
              <mat-icon class="shield-icon">shield_lock</mat-icon>
            </div>
            <div class="shield-pulse"></div>
          </div>
          <h1 class="viewer-title">Welcome, {{ userName }}!</h1>
          <p class="viewer-message">Due to high security protocols, you are currently not authorized to access the system features.</p>
          <p class="viewer-submessage">Please contact your administrator to request the necessary permissions.</p>
          <div class="viewer-divider"></div>
          <button mat-flat-button color="primary" class="viewer-btn" (click)="showThankYou = true" *ngIf="!showThankYou">
            <mat-icon>check_circle</mat-icon>
            I Understand
          </button>
          <div class="thank-you-block" *ngIf="showThankYou">
            <mat-icon class="heart-icon">favorite</mat-icon>
            <p class="thank-title">Thank you for your understanding!</p>
            <p class="thank-subtitle">We wish you a wonderful day! 🌟</p>
            <p class="thank-note">Your administrator will review your access soon.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ========== NORMAL DASHBOARD FOR ADMIN/TEST/SUPERADMIN ========== -->
    <ng-container *ngIf="!isViewerOnly">
      <app-page-header title="Executive Dashboard" subtitle="Real-time overview of all projects and contractors">
      </app-page-header>

      <div class="dashboard-container">
        <!-- Loading State -->
        <div *ngIf="isLoading" class="loading-container">
          <div class="spinner-text">
            <mat-spinner diameter="48" color="primary"></mat-spinner>
            <span>Loading dashboard data...</span>
          </div>
        </div>

        <!-- Error State -->
        <div *ngIf="hasError && !isLoading" class="error-container">
          <mat-icon>error_outline</mat-icon>
          <h3>Failed to Load Dashboard</h3>
          <p>{{ errorMessage }}</p>
          <button mat-raised-button color="primary" (click)="loadDashboard()">
            <mat-icon>refresh</mat-icon>
            Retry
          </button>
        </div>

        <!-- Dashboard Content -->
        <ng-container *ngIf="dashboard && !isLoading">
          <!-- Stats Row -->
          <div class="stats-grid">
            <app-stat-card
              label="Total Projects"
              [value]="dashboard.totalProjects"
              icon="business"
              colorClass="primary"
              [footerText]="'Active: ' + dashboard.activeProjects + ' | Completed: ' + dashboard.completedProjects">
            </app-stat-card>

            <app-stat-card
              label="Total Contractors"
              [value]="dashboard.totalContractors"
              icon="groups"
              colorClass="success">
            </app-stat-card>

            <app-stat-card
              label="Total Budget"
              [value]="dashboard.totalBudget"
              icon="account_balance_wallet"
              colorClass="info"
              [footerText]="'Spent: ' + formatCurrency(dashboard.totalSpent)">
            </app-stat-card>

            <app-stat-card
              label="Budget Utilization"
              [value]="dashboard.budgetUtilization"
              icon="trending_up"
              colorClass="warning"
              [footerText]="'Avg Progress: ' + dashboard.averageProgress + '%'">
            </app-stat-card>

            <app-stat-card
              label="Delayed Projects"
              [value]="dashboard.delayedProjects"
              icon="warning"
              colorClass="danger"
              [footerText]="'Pending Approvals: ' + dashboard.pendingApprovals">
            </app-stat-card>

            <app-stat-card
              label="Active Performance Bonds"
              [value]="dashboard.activePerformanceBonds"
              icon="verified"
              colorClass="purple"
              [footerText]="'Expiring Guarantees: ' + dashboard.expiringGuarantees">
            </app-stat-card>
          </div>

          <!-- Charts Row -->
          <div class="charts-row">
            <!-- Status Distribution Pie Chart -->
            <div class="chart-card">
              <div class="chart-title">
                <mat-icon>pie_chart</mat-icon>
                Project Status Distribution
              </div>
              <div class="chart-container">
                <ngx-charts-pie-chart
                  *ngIf="statusChartData.length > 0"
                  [results]="statusChartData"
                  [scheme]="colorScheme"
                  [labels]="true"
                  [doughnut]="true"
                  [arcWidth]="0.4"
                  [animations]="true"
                  [tooltipDisabled]="false"
                  style="width: 100%; height: 100%;">
                </ngx-charts-pie-chart>
                <p *ngIf="statusChartData.length === 0" style="color: #9e9e9e;">No data available</p>
              </div>
            </div>

            <!-- Monthly Progress Line Chart -->
            <div class="chart-card">
              <div class="chart-title">
                <mat-icon>show_chart</mat-icon>
                Monthly Progress (Planned vs Actual)
              </div>
              <div class="chart-container">
                <ngx-charts-line-chart
                  *ngIf="progressChartData.length > 0"
                  [results]="progressChartData"
                  [scheme]="colorScheme"
                  [legend]="true"
                  [xAxis]="true"
                  [yAxis]="true"
                  [animations]="true"
                  [autoScale]="true"
                  style="width: 100%; height: 100%;">
                </ngx-charts-line-chart>
                <p *ngIf="progressChartData.length === 0" style="color: #9e9e9e;">No data available</p>
              </div>
            </div>
          </div>

          <!-- Bottom Row -->
          <div class="bottom-row">
            <!-- Top Projects by Budget -->
            <div class="chart-card">
              <div class="chart-title">
                <mat-icon>bar_chart</mat-icon>
                Top Projects by Budget
              </div>
              <div class="chart-container">
                <ngx-charts-bar-vertical
                  *ngIf="budgetChartData.length > 0"
                  [results]="budgetChartData"
                  [scheme]="colorScheme"
                  [xAxis]="true"
                  [yAxis]="true"
                  [animations]="true"
                  style="width: 100%; height: 100%;">
                </ngx-charts-bar-vertical>
                <p *ngIf="budgetChartData.length === 0" style="color: #9e9e9e;">No data available</p>
              </div>
            </div>

            <!-- Recent Delays Table -->
            <div class="table-card">
              <div class="table-header">
                <div class="table-title">
                  <mat-icon>warning</mat-icon>
                  Recent Delays
                </div>
              </div>
              <div style="overflow-x: auto;">
                <table class="mini-table" *ngIf="dashboard.recentDelays && dashboard.recentDelays.length > 0">
                  <thead>
                    <tr>
                      <th>Project</th>
                      <th>Contractor</th>
                      <th>Days</th>
                      <th>Reason</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let delay of dashboard.recentDelays">
                      <td>{{ delay.projectName }}</td>
                      <td>{{ delay.contractorName }}</td>
                      <td>
                        <span class="delay-days-badge" [ngClass]="getDelayClass(delay.days)">
                          {{ delay.days }} days
                        </span>
                      </td>
                      <td>{{ delay.reason }}</td>
                    </tr>
                  </tbody>
                </table>
                <div *ngIf="!dashboard.recentDelays || dashboard.recentDelays.length === 0" style="padding: 32px; text-align: center; color: #9e9e9e;">
                  No recent delays recorded
                </div>
              </div>
            </div>
          </div>

          <!-- Top Projects Table -->
          <div class="table-card" style="margin-bottom: 0;">
            <div class="table-header">
              <div class="table-title">
                <mat-icon>star</mat-icon>
                Top Projects by Budget
              </div>
            </div>
            <div style="overflow-x: auto;">
              <table class="mini-table" *ngIf="dashboard.topProjectsByBudget && dashboard.topProjectsByBudget.length > 0">
                <thead>
                  <tr>
                    <th>Project Name</th>
                    <th>Budget</th>
                    <th>Spent</th>
                    <th>Progress</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let project of dashboard.topProjectsByBudget">
                    <td>{{ project.projectName }}</td>
                    <td>{{ formatCurrency(project.budget) }}</td>
                    <td>{{ formatCurrency(project.spent) }}</td>
                    <td>
                      <div class="progress-indicator">
                        <mat-progress-bar
                          mode="determinate"
                          [value]="project.progress"
                          [color]="getProgressColor(project.progress)">
                        </mat-progress-bar>
                        <span>{{ project.progress }}%</span>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
              <div *ngIf="!dashboard.topProjectsByBudget || dashboard.topProjectsByBudget.length === 0" style="padding: 32px; text-align: center; color: #9e9e9e;">
                No project data available
              </div>
            </div>
          </div>
        </ng-container>
      </div>
    </ng-container>
  `,
  styles: [`
    /* ========== VIEWER ACCESS DENIED PAGE ========== */
    .viewer-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: calc(100vh - 64px);
      background: linear-gradient(160deg, #f0f4f8 0%, #e2e8f0 30%, #f5f7fa 60%, #edf2f7 100%);
      padding: 32px;
    }

    .viewer-container {
      width: 100%;
      max-width: 600px;
    }

    .viewer-card {
      background: #ffffff;
      border-radius: 24px;
      padding: 56px 48px;
      text-align: center;
      box-shadow: 0 24px 80px rgba(0, 0, 0, 0.08), 0 4px 16px rgba(0, 0, 0, 0.04);
      animation: cardEnter 0.7s cubic-bezier(0.22, 0.61, 0.36, 1);
      border: 1px solid rgba(0, 0, 0, 0.04);
    }

    @keyframes cardEnter {
      from {
        opacity: 0;
        transform: translateY(40px) scale(0.95);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }

    .shield-wrapper {
      position: relative;
      width: 100px;
      height: 100px;
      margin: 0 auto 28px;
    }

    .shield-outer {
      width: 100px;
      height: 100px;
      border-radius: 50%;
      background: linear-gradient(135deg, #ff9800, #f57c00);
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;
      z-index: 2;
      box-shadow: 0 8px 32px rgba(245, 124, 0, 0.3);
    }

    .shield-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #fff;
    }

    .shield-pulse {
      position: absolute;
      top: -12px;
      left: -12px;
      right: -12px;
      bottom: -12px;
      border-radius: 50%;
      border: 2px solid rgba(245, 124, 0, 0.3);
      animation: pulse 2s ease-in-out infinite;
      z-index: 1;
    }

    @keyframes pulse {
      0%, 100% {
        transform: scale(1);
        opacity: 0.6;
      }
      50% {
        transform: scale(1.25);
        opacity: 0;
      }
    }

    .viewer-title {
      font-size: 1.6rem;
      font-weight: 700;
      color: #1a1a2e;
      margin: 0 0 14px;
      line-height: 1.3;
    }

    .viewer-message {
      font-size: 1rem;
      color: #555;
      margin: 0 0 6px;
      line-height: 1.7;
    }

    .viewer-submessage {
      font-size: 0.9rem;
      color: #888;
      margin: 0 0 28px;
      line-height: 1.6;
    }

    .viewer-divider {
      width: 50px;
      height: 3px;
      background: linear-gradient(90deg, #1a73e8, #64b5f6);
      border-radius: 2px;
      margin: 0 auto 28px;
    }

    .viewer-btn {
      padding: 12px 36px;
      font-size: 1rem;
      font-weight: 500;
      border-radius: 30px;
      height: 48px;
      transition: all 0.3s;
    }

    .viewer-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 10px 28px rgba(26, 115, 232, 0.35);
    }

    .thank-you-block {
      animation: fadeUp 0.5s ease-out;
    }

    @keyframes fadeUp {
      from {
        opacity: 0;
        transform: translateY(16px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .heart-icon {
      font-size: 44px;
      width: 44px;
      height: 44px;
      color: #e91e63;
      margin-bottom: 12px;
      animation: heartBeat 1.2s ease-in-out infinite;
    }

    @keyframes heartBeat {
      0%, 100% {
        transform: scale(1);
      }
      50% {
        transform: scale(1.2);
      }
    }

    .thank-title {
      font-size: 1.15rem;
      font-weight: 600;
      color: #333;
      margin: 0 0 4px;
    }

    .thank-subtitle {
      font-size: 0.95rem;
      color: #666;
      margin: 0 0 8px;
    }

    .thank-note {
      font-size: 0.8rem;
      color: #999;
      margin: 0;
    }

    /* Dark Theme for Viewer Page */
    body.dark-theme .viewer-page {
      background: linear-gradient(160deg, #0d1117 0%, #161b22 40%, #0d1117 100%);
    }
    body.dark-theme .viewer-card {
      background: #161b22;
      border-color: #21262d;
    }
    body.dark-theme .viewer-title {
      color: #e6edf3;
    }
    body.dark-theme .viewer-message {
      color: #8b949e;
    }
    body.dark-theme .viewer-submessage {
      color: #6e7681;
    }
    body.dark-theme .thank-title {
      color: #e6edf3;
    }
    body.dark-theme .thank-subtitle {
      color: #8b949e;
    }
    body.dark-theme .thank-note {
      color: #6e7681;
    }

    /* ========== NORMAL DASHBOARD ========== */
    .dashboard-container {
      padding: 24px;
      animation: fadeIn 0.3s ease-in-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .charts-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
      margin-bottom: 24px;
    }

    .chart-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
      padding: 20px;
    }

    .chart-card.full-width {
      grid-column: 1 / -1;
    }

    .chart-title {
      font-size: 1rem;
      font-weight: 600;
      color: #212121;
      margin-bottom: 16px;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .chart-title mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
      color: #1a73e8;
    }

    .chart-container {
      height: 320px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .bottom-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
      margin-bottom: 24px;
    }

    .table-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
      overflow: hidden;
    }

    .table-header {
      padding: 16px 20px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      border-bottom: 1px solid #f0f0f0;
    }

    .table-title {
      font-size: 1rem;
      font-weight: 600;
      color: #212121;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .table-title mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
      color: #1a73e8;
    }

    .mini-table {
      width: 100%;
      border-collapse: collapse;
    }

    .mini-table th {
      background: #fafafa;
      padding: 10px 16px;
      font-size: 0.75rem;
      font-weight: 600;
      color: #757575;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      text-align: left;
      border-bottom: 2px solid #e0e0e0;
    }

    .mini-table td {
      padding: 12px 16px;
      font-size: 0.85rem;
      color: #424242;
      border-bottom: 1px solid #f5f5f5;
    }

    .mini-table tr:hover td {
      background: #fafafa;
    }

    .progress-indicator {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .progress-indicator mat-progress-bar {
      flex: 1;
      max-width: 100px;
    }

    .progress-indicator span {
      font-size: 0.8rem;
      font-weight: 600;
      color: #616161;
      min-width: 36px;
    }

    .loading-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 400px;
    }

    .spinner-text {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
      color: #757575;
    }

    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 400px;
      gap: 16px;
    }

    .error-container mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #d32f2f;
    }

    .error-container button {
      margin-top: 8px;
    }

    .delay-days-badge {
      display: inline-block;
      padding: 2px 10px;
      border-radius: 12px;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .delay-critical { background: #ffebee; color: #c62828; }
    .delay-warning { background: #fff3e0; color: #e65100; }
    .delay-moderate { background: #fff8e1; color: #f57f17; }

    @media (max-width: 1024px) {
      .charts-row,
      .bottom-row {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 600px) {
      .dashboard-container {
        padding: 16px;
      }

      .stats-grid {
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
        gap: 8px;
      }

      .viewer-card {
        padding: 36px 24px;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly auth = inject(AuthService);

  dashboard: ExecutiveDashboard | null = null;
  isLoading = true;
  hasError = false;
  errorMessage: string = '';

  isViewerOnly = false;
  userName = '';
  showThankYou = false;

  statusChartData: { name: string; value: number }[] = [];
  progressChartData: { name: string; series: { name: string; value: number }[] }[] = [];
  budgetChartData: { name: string; value: number }[] = [];

  colorScheme: any = {
    domain: ['#388e3c', '#1976d2', '#f57c00', '#d32f2f', '#9c27b0', '#757575']
  };

  ngOnInit(): void {
    const user = this.auth.getCurrentUser();
    this.userName = user?.firstName || 'User';
    const roles = user?.roles || [];
    this.isViewerOnly = roles.length === 1 && roles[0] === 'Viewer';

    if (!this.isViewerOnly) {
      this.loadDashboard();
    }
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.hasError = false;

    this.dashboardService.getExecutiveDashboard().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.dashboard = response.data;
          this.prepareChartData();
        } else {
          this.hasError = true;
          this.errorMessage = response.message || 'Failed to load dashboard data';
        }
        this.isLoading = false;
      },
      error: () => {
        this.hasError = true;
        this.errorMessage = 'Unable to connect to server. Please try again.';
        this.isLoading = false;
      },
    });
  }

  prepareChartData(): void {
    if (!this.dashboard) return;

    this.statusChartData = (this.dashboard.projectStatusDistribution || []).map((item) => ({
      name: item.status,
      value: item.count,
    }));

    if (this.dashboard.monthlyProgress && this.dashboard.monthlyProgress.length > 0) {
      this.progressChartData = [
        {
          name: 'Planned',
          series: this.dashboard.monthlyProgress.map((m) => ({
            name: m.month,
            value: m.planned,
          })),
        },
        {
          name: 'Actual',
          series: this.dashboard.monthlyProgress.map((m) => ({
            name: m.month,
            value: m.actual,
          })),
        },
      ];
    }

    this.budgetChartData = (this.dashboard.topProjectsByBudget || []).map((p) => ({
      name: p.projectName,
      value: p.budget,
    }));
  }

  formatCurrency(value: number): string {
    if (value >= 10000000) return '₹' + (value / 10000000).toFixed(1) + ' Cr';
    if (value >= 100000) return '₹' + (value / 100000).toFixed(1) + ' L';
    return '₹' + value.toLocaleString('en-IN');
  }

  getDelayClass(days: number): string {
    if (days > 30) return 'delay-critical';
    if (days > 15) return 'delay-warning';
    return 'delay-moderate';
  }

  getProgressColor(progress: number): string {
    if (progress >= 80) return 'primary';
    if (progress >= 50) return 'accent';
    return 'warn';
  }
}