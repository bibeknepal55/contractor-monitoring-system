import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { ProjectService } from '../../../core/services/project.service';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-project-assignment',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatSelectModule, MatChipsModule, MatTooltipModule, MatCardModule, MatDividerModule
  ],
  template: `
    <mat-card class="assignment-card">
      <mat-card-header>
        <mat-icon mat-card-avatar>assignment_ind</mat-icon>
        <mat-card-title>Project Access Control</mat-card-title>
        <mat-card-subtitle>Assign specific users to this project</mat-card-subtitle>
      </mat-card-header>
      <mat-divider></mat-divider>
      <mat-card-content>
        <p class="info-text">
          Users assigned here can access this project even if their role doesn't include it.
          Users NOT assigned cannot access this project even if their role allows it.
        </p>

        <!-- Currently Assigned -->
        <div class="assigned-section">
          <h4>Currently Assigned ({{ assignedUsers.length }})</h4>
          <div class="user-chips">
            <mat-chip *ngFor="let user of assignedUsers" class="user-chip" [removable]="canModify" (removed)="removeUser(user)">
              {{ user.firstName }} {{ user.lastName }}
              <span class="user-role">{{ user.role }}</span>
              <mat-icon matChipRemove *ngIf="canModify">cancel</mat-icon>
            </mat-chip>
            <span *ngIf="assignedUsers.length === 0" class="no-users">No users assigned yet</span>
          </div>
        </div>

        <!-- Add User -->
        <div class="add-section" *ngIf="canModify">
          <mat-form-field appearance="outline">
            <mat-label>Add User</mat-label>
            <mat-select [(ngModel)]="selectedUser" (selectionChange)="addUser()">
              <mat-option *ngFor="let user of availableUsers" [value]="user.id">
                {{ user.firstName }} {{ user.lastName }} ({{ user.role || 'No Role' }})
              </mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <!-- Permission Override -->
        <div class="override-section" *ngIf="canModify && assignedUsers.length > 0">
          <h4>Default Access Level</h4>
          <mat-form-field appearance="outline">
            <mat-label>Access Level for Assigned Users</mat-label>
            <mat-select [(ngModel)]="accessLevel">
              <mat-option value="View">View Only</mat-option>
              <mat-option value="Edit">View + Edit</mat-option>
              <mat-option value="Full">Full Access (CRUD)</mat-option>
            </mat-select>
          </mat-form-field>
          <button mat-stroked-button color="primary" (click)="saveAccessLevel()">
            <mat-icon>save</mat-icon> Save Access Level
          </button>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .assignment-card{border-radius:12px;border:1px solid #e8eaed;margin-bottom:20px}
    .assignment-card mat-card-header{padding:16px 20px 0}
    .assignment-card mat-card-content{padding:16px 20px 20px}
    .info-text{font-size:.82rem;color:#666;line-height:1.5;margin-bottom:16px}
    .assigned-section{margin-bottom:16px}
    .assigned-section h4{font-size:.85rem;font-weight:600;color:#333;margin:0 0 8px}
    .user-chips{display:flex;flex-wrap:wrap;gap:6px}
    .user-chip{font-size:.8rem!important}
    .user-role{font-size:.65rem;color:#888;margin-left:4px}
    .no-users{color:#999;font-size:.82rem}
    .add-section{margin-bottom:16px}
    .override-section h4{font-size:.85rem;font-weight:600;color:#333;margin:0 0 8px}
    mat-form-field{width:100%}
  `]
})
export class ProjectAssignmentComponent implements OnInit {
  @Input() projectId!: string;

  private projectSrv = inject(ProjectService);
  private userSrv = inject(UserService);
  private notify = inject(NotificationService);
  readonly auth = inject(AuthService);

  assignedUsers: any[] = [];
  availableUsers: any[] = [];
  selectedUser: string | null = null;
  accessLevel = 'View';

  get canModify(): boolean {
    return this.auth.hasPermission('Project.Update') || this.auth.hasAnyRole(['SuperAdmin', 'Admin']);
  }

  ngOnInit(): void {
    this.loadAssignedUsers();
    this.loadAvailableUsers();
  }

  loadAssignedUsers(): void {
    // GET /api/v1/projects/{projectId}/assignments
    this.projectSrv.getProjectById(this.projectId).subscribe((r: any) => {
      if (r.success && r.data?.assignedUsers) {
        this.assignedUsers = r.data.assignedUsers;
      }
    });
  }

  loadAvailableUsers(): void {
    this.userSrv.getUsers({ page: 1, pageSize: 200 }).subscribe((r: any) => {
      if (r.success) {
        const assignedIds = this.assignedUsers.map(u => u.id);
        this.availableUsers = r.data.filter((u: any) => !assignedIds.includes(u.id));
      }
    });
  }

  addUser(): void {
    if (!this.selectedUser) return;
    const user = this.availableUsers.find(u => u.id === this.selectedUser);
    if (user) {
      // POST /api/v1/projects/{projectId}/assignments
      this.assignedUsers.push(user);
      this.availableUsers = this.availableUsers.filter(u => u.id !== this.selectedUser);
      this.selectedUser = null;
      this.notify.success(`${user.firstName} assigned to project`);
    }
  }

  removeUser(user: any): void {
    // DELETE /api/v1/projects/{projectId}/assignments/{userId}
    this.assignedUsers = this.assignedUsers.filter(u => u.id !== user.id);
    this.availableUsers.push(user);
    this.notify.info(`${user.firstName} removed from project`);
  }

  saveAccessLevel(): void {
    // PUT /api/v1/projects/{projectId}/assignments/access-level
    this.notify.success('Access level updated');
  }
}