import { Component, inject, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { AuthService } from '../../core/services/auth.service';
import { TrainingService } from '../../core/services/training.service';

interface TrainingModule {
  name: string;
  icon: string;
  steps: string[];
  tip?: string;
}

@Component({
  selector: 'app-training-panel',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatCardModule, MatDividerModule, MatProgressBarModule, MatTooltipModule, MatExpansionModule],
  template: `
    <div class="training-overlay" (click)="close.emit()">
      <div class="training-panel" (click)="$event.stopPropagation()">
        <!-- Header -->
        <div class="panel-header">
          <div class="header-left">
            <mat-icon class="header-icon">school</mat-icon>
            <div>
              <h2>Training Session</h2>
              <p class="header-subtitle">Welcome, {{ userName }} • {{ userRole }}</p>
            </div>
          </div>
          <button mat-icon-button (click)="close.emit()" class="close-btn">
            <mat-icon>close</mat-icon>
          </button>
        </div>

        <!-- Progress Bar -->
        <div class="progress-section">
          <div class="progress-info">
            <span>Progress</span>
            <span>{{ completionPercentage }}% complete</span>
          </div>
          <mat-progress-bar mode="determinate" [value]="completionPercentage" color="primary"></mat-progress-bar>
        </div>

        <mat-divider></mat-divider>

        <!-- Module List -->
        <div class="panel-body">
          <p class="intro-text">
            Learn how to use each module available to your role. Click on a module to expand and view step-by-step instructions.
          </p>

          <mat-accordion class="module-accordion">
            <mat-expansion-panel *ngFor="let module of trainingModules" 
              (opened)="onModuleOpened(module.name)"
              [class.completed]="isModuleCompleted(module.name)">
              
              <mat-expansion-panel-header>
                <mat-panel-title>
                  <mat-icon class="module-icon">{{ module.icon }}</mat-icon>
                  <span class="module-name">{{ module.name }}</span>
                  <mat-icon *ngIf="isModuleCompleted(module.name)" class="check-icon">check_circle</mat-icon>
                </mat-panel-title>
              </mat-expansion-panel-header>

              <div class="module-content">
                <ol class="step-list">
                  <li *ngFor="let step of module.steps">{{ step }}</li>
                </ol>
                <div class="tip-box" *ngIf="module.tip">
                  <mat-icon>lightbulb</mat-icon>
                  <span>{{ module.tip }}</span>
                </div>
                <button mat-stroked-button color="primary" 
                  (click)="markComplete(module.name); $event.stopPropagation()"
                  *ngIf="!isModuleCompleted(module.name)" class="complete-btn">
                  <mat-icon>check</mat-icon> Mark as Complete
                </button>
              </div>
            </mat-expansion-panel>
          </mat-accordion>
        </div>

        <!-- Footer -->
        <mat-divider></mat-divider>
        <div class="panel-footer">
          <button mat-stroked-button (click)="close.emit()">Close</button>
          <button mat-flat-button color="primary" (click)="markAllComplete()" 
            *ngIf="completionPercentage < 100">
            <mat-icon>done_all</mat-icon> Mark All Complete
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .training-overlay {
      position: fixed; top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5); z-index: 1200;
      display: flex; justify-content: flex-end;
      animation: fadeIn 0.2s ease;
    }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

    .training-panel {
      width: 600px; max-width: 93vw; background: #fff; height: 100%;
      overflow-y: auto; box-shadow: -10px 0 40px rgba(0,0,0,0.2);
      animation: slideIn 0.3s ease; display: flex; flex-direction: column;
    }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }

    .panel-header {
      display: flex; justify-content: space-between; align-items: center;
      padding: 20px 24px; background: linear-gradient(135deg, #0d47a1, #1a73e8);
      color: white; flex-shrink: 0;
    }
    .header-left { display: flex; align-items: center; gap: 12px; }
    .header-icon { font-size: 32px; width: 32px; height: 32px; color: #ffd54f; }
    .header-left h2 { font-size: 1.2rem; font-weight: 650; margin: 0; }
    .header-subtitle { font-size: 0.78rem; opacity: 0.9; margin: 2px 0 0; }
    .close-btn { color: white; }

    .progress-section { padding: 16px 24px; flex-shrink: 0; }
    .progress-info { display: flex; justify-content: space-between; font-size: 0.82rem; color: #666; margin-bottom: 6px; }

    .panel-body { padding: 16px 20px; flex: 1; overflow-y: auto; }
    .intro-text { font-size: 0.88rem; color: #666; margin: 0 0 16px; line-height: 1.5; }

    .module-accordion { display: flex; flex-direction: column; gap: 6px; }
    .module-accordion mat-expansion-panel {
      border-radius: 10px !important; border: 1px solid #e5e7eb;
      box-shadow: none !important; margin: 0;
    }
    .module-accordion mat-expansion-panel.completed {
      border-color: #a7f3d0; background: #f9fefb;
    }
    .module-accordion mat-expansion-panel-header { padding: 0 16px; font-size: 0.9rem; }
    .module-icon { font-size: 20px; width: 20px; height: 20px; color: #1a73e8; margin-right: 10px; }
    .module-name { font-weight: 500; }
    .check-icon { font-size: 18px; width: 18px; height: 18px; color: #059669; margin-left: auto; }

    .module-content { padding: 8px 16px 16px; }
    .step-list { margin: 0; padding-left: 18px; }
    .step-list li { padding: 4px 0; font-size: 0.85rem; color: #444; line-height: 1.4; }
    .tip-box {
      display: flex; align-items: flex-start; gap: 8px;
      background: #fffbeb; border-radius: 8px; padding: 10px 14px;
      margin-top: 12px; border: 1px solid #fde68a;
    }
    .tip-box mat-icon { font-size: 18px; width: 18px; height: 18px; color: #f59e0b; flex-shrink: 0; }
    .tip-box span { font-size: 0.82rem; color: #92400e; }
    .complete-btn { margin-top: 12px; font-size: 0.8rem; }

    .panel-footer {
      display: flex; justify-content: space-between; align-items: center;
      padding: 14px 24px; flex-shrink: 0; background: #fafafa;
    }

    @media (max-width: 768px) {
      .training-panel { width: 100vw; }
    }
  `]
})
export class TrainingPanelComponent implements OnInit {
  @Input() show = false;
  @Output() close = new EventEmitter<void>();

  private auth = inject(AuthService);
  private trainingSrv = inject(TrainingService);

  userName = '';
  userRole = '';
  trainingModules: TrainingModule[] = [];
  completionPercentage = 0;

  ngOnInit(): void {
    const user = this.auth.getCurrentUser();
    this.userName = user ? `${user.firstName} ${user.lastName}` : 'User';
    this.userRole = this.auth.getHighestRole() || 'Viewer';
    this.trainingModules = this.getModulesForRole(this.userRole);
    this.updateProgress();
  }

  getModulesForRole(role: string): TrainingModule[] {
    const common: TrainingModule[] = [
      {
        name: 'Dashboard', icon: 'dashboard',
        steps: [
          'Click "Dashboard" in the sidebar menu',
          'View executive charts showing project status, contractor summary, and financial overview',
          'Click on any chart to see detailed data',
          'Use the date filter to view different time periods'
        ],
        tip: 'The dashboard updates automatically when you create or modify records.'
      },
      {
        name: 'Projects', icon: 'business',
        steps: [
          'Click "Projects" in the sidebar',
          'View the table showing all projects with code, name, status, budget',
          'Click "+ New Project" button to create a new project',
          'Fill in project code, name, location, budget, and dates',
          'Use the search bar to find specific projects',
          'Click on a row to view details, or use Edit/Delete icons'
        ],
        tip: 'Project Code is required and must be unique.'
      },
      {
        name: 'Contractors', icon: 'groups',
        steps: [
          'Click "Contractors" in the sidebar',
          'View all registered contractors with their details',
          'Click "+ New Contractor" to add a new contractor',
          'Fill in company name, contact info, and registration details',
          'Use Edit ✏️ or Delete 🗑️ icons to manage entries'
        ],
        tip: 'Each contractor can be linked to multiple projects.'
      },
      {
        name: 'Business Modules', icon: 'folder',
        steps: [
          'Expand "Business Modules" in the sidebar to see all sub-modules',
          'Each sub-module manages a specific aspect of contractor monitoring',
          'Click any module to view its table of records',
          'Use the "+" button to create new entries',
          'All modules support Create, Read, Update, and Delete operations'
        ],
        tip: 'Business modules include Financials, Bonds, Guarantees, Progress, and more.'
      },
      {
        name: 'Contract Financials', icon: 'account_balance',
        steps: [
          'Navigate to Business Modules → Contract Financials',
          'View financial details linked to each contract',
          'Create new financial entries with contract amounts, payments, and balances',
          'Edit or delete entries as needed'
        ]
      },
      {
        name: 'Performance Bonds', icon: 'verified',
        steps: [
          'Navigate to Business Modules → Performance Bonds',
          'View all performance bonds issued',
          'Create new bonds with bond number, amount, and validity dates',
          'Track bond status and expiry'
        ]
      },
      {
        name: 'Advance Payment Guarantees', icon: 'shield',
        steps: [
          'Navigate to Business Modules → Advance Payment Guarantees',
          'View all APG records',
          'Create new guarantees with guarantee number and amount',
          'Monitor guarantee status and validity'
        ]
      },
      {
        name: 'Physical Progress', icon: 'trending_up',
        steps: [
          'Navigate to Business Modules → Physical Progress',
          'Track physical progress of projects in percentage',
          'Update progress regularly to keep reports accurate',
          'View progress history for each project'
        ]
      },
      {
        name: 'Time Extensions', icon: 'schedule',
        steps: [
          'Navigate to Business Modules → Time Extensions',
          'View all time extension requests',
          'Create new extension requests with reason and duration',
          'Track approval status of each request'
        ]
      },
      {
        name: 'Delay Reasons', icon: 'warning',
        steps: [
          'Navigate to Business Modules → Delay Reasons',
          'Document reasons for project delays',
          'Create entries with delay type, description, and impact',
          'Use this data for reports and analysis'
        ]
      },
      {
        name: 'Raw Materials', icon: 'inventory',
        steps: [
          'Navigate to Business Modules → Raw Materials',
          'Track raw materials used across projects',
          'Add materials with name, quantity, and unit',
          'Monitor material consumption per project'
        ]
      },
      {
        name: 'Lab Tests', icon: 'science',
        steps: [
          'Navigate to Business Modules → Lab Tests',
          'Record lab test results for materials and works',
          'Create entries with test name, result, and date',
          'Upload test reports if needed'
        ]
      },
      {
        name: 'Photo Monitoring', icon: 'photo_camera',
        steps: [
          'Navigate to Business Modules → Photo Monitoring',
          'Upload photos to document project progress',
          'Add descriptions and dates to each photo',
          'View photo gallery for each project'
        ]
      },
      {
        name: 'Subcontractors', icon: 'handshake',
        steps: [
          'Navigate to Business Modules → Subcontractors',
          'Manage subcontractor information',
          'Add subcontractors with company details and scope of work',
          'Link subcontractors to specific projects'
        ]
      },
      {
        name: 'Responsible Officials', icon: 'badge',
        steps: [
          'Navigate to Business Modules → Responsible Officials',
          'Assign responsible officials to projects',
          'Track official names, positions, and contact information',
          'Manage assignments per project'
        ]
      },
      {
        name: 'Approval Workflow', icon: 'fact_check',
        steps: [
          'Click "Approval Workflow" in the sidebar',
          'View all approval requests you have submitted',
          'Click "New Request" to submit an item for approval',
          'Select the module, record, and add comments',
          'Track the status: Pending, Approved, or Rejected',
          'Note: Only Admin/SuperAdmin can approve or reject requests'
        ],
        tip: 'You can edit your own pending requests before they are approved.'
      },
      {
        name: 'Reports & Analytics', icon: 'assessment',
        steps: [
          'Click "Reports" in the sidebar',
          'Select a report type from the available options',
          'Apply filters like date range, project, or contractor',
          'Click "Generate" to view the report',
          'Export reports to Excel or PDF for sharing'
        ],
        tip: 'Reports help you analyze project performance and contractor status.'
      }
    ];

    const adminModules: TrainingModule[] = [
      {
        name: 'User Management', icon: 'people',
        steps: [
          'Click "User Management" in the sidebar',
          'View all users in the system with their roles and status',
          'Create new users with appropriate roles',
          'Edit user details or change their status (Active/Inactive)',
          'Delete users if needed (restricted by role hierarchy)'
        ],
        tip: 'Admins cannot manage SuperAdmin accounts.'
      },
      {
        name: 'User Activity Logs', icon: 'history',
        steps: [
          'Navigate to User Activity → User Logs',
          'View all user actions recorded in the system',
          'See who logged in, what they created/updated/deleted',
          'Use filters to find specific activities by date, type, or module',
          'Click any row to see full request details including request body',
          'Export activity logs to Excel or PDF for compliance',
          'Stats cards show Logins, Active Users, Failed Logins, and Actions'
        ],
        tip: 'Clear History button (SuperAdmin only) permanently deletes all logs.'
      }
    ];

    const superAdminModules: TrainingModule[] = [
      {
        name: 'Role Management', icon: 'admin_panel_settings',
        steps: [
          'Click "Role Management" in the sidebar',
          'View all roles and their assigned permissions',
          'Edit role permissions by checking/unchecking specific actions',
          'Create custom roles if needed',
          'Changes take effect immediately for all users with that role'
        ],
        tip: 'Be careful when modifying SuperAdmin permissions.'
      }
    ];

    if (role === 'SuperAdmin') return [...common, ...adminModules, ...superAdminModules];
    if (role === 'Admin') return [...common, ...adminModules];
    return common;
  }

  isModuleCompleted(name: string): boolean {
    const progress = this.trainingSrv.getProgress();
    return progress.completed || progress.completedModules.includes(name);
  }

  onModuleOpened(name: string): void {
    // Track that user viewed this module
  }

  markComplete(name: string): void {
    this.trainingSrv.markModuleComplete(name);
    this.updateProgress();
  }

  markAllComplete(): void {
    this.trainingSrv.markAllComplete();
    this.updateProgress();
  }

  updateProgress(): void {
    this.completionPercentage = this.trainingSrv.getCompletionPercentage(this.trainingModules.length);
  }
}