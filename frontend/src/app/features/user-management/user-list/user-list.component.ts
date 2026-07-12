import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, MatButtonModule, MatIconModule,
    MatTableModule, MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatProgressBarModule, MatTooltipModule, MatCardModule, MatChipsModule,
    MatDividerModule
  ],
  template: `
    <div class="page">
      <div class="header">
        <div><h1>User Management</h1><p>{{total}} users total</p></div>
      </div>

      <div class="filters">
        <mat-form-field appearance="outline" class="sf">
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="searchText" (ngModelChange)="onSearch($event)" placeholder="Search users...">
          <button mat-icon-button matSuffix *ngIf="searchText" (click)="searchText='';fetch()"><mat-icon>close</mat-icon></button>
        </mat-form-field>
        <mat-form-field appearance="outline" style="width:150px">
          <mat-label>Status</mat-label>
          <mat-select [(ngModel)]="statusFilter" (selectionChange)="fetch()">
            <mat-option value="">All</mat-option>
            <mat-option value="true">Active</mat-option>
            <mat-option value="false">Inactive</mat-option>
          </mat-select>
        </mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh"><mat-icon>refresh</mat-icon></button>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>

      <div class="table-wrap" *ngIf="!loading && users.length > 0">
        <table mat-table [dataSource]="users" matSort (matSortChange)="onSort($event)">
          
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>User</th>
            <td mat-cell *matCellDef="let u">
              <strong>{{u.firstName}} {{u.lastName}}</strong>
              <br><small style="color:#999;">{{u.email}}</small>
            </td>
          </ng-container>

          <ng-container matColumnDef="roles">
            <th mat-header-cell *matHeaderCellDef>Roles</th>
            <td mat-cell *matCellDef="let u">
              <mat-chip *ngFor="let role of u.roles" class="role-chip" 
                [style.background-color]="roleColor(role)+'20'" [style.color]="roleColor(role)" selected>
                {{role}}
              </mat-chip>
              <span *ngIf="!u.roles || u.roles.length === 0" style="color:#999;">No roles</span>
            </td>
          </ng-container>

          <ng-container matColumnDef="isActive">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let u">
              <span class="badge" [class.active]="u.isActive" [class.inactive]="!u.isActive">
                {{u.isActive ? 'Active' : 'Inactive'}}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="createdAt">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Joined</th>
            <td mat-cell *matCellDef="let u">{{u.createdAt ? formatDate(u.createdAt) : '-'}}</td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef style="text-align:center;">Actions</th>
            <td mat-cell *matCellDef="let u" style="text-align:center;">
              
              <!-- Manage Roles: SuperAdmin=all, Admin=Test/Viewer only -->
              <button mat-icon-button color="primary" (click)="openRoleDialog(u)" 
                *ngIf="canManageRoles(u)" matTooltip="Manage Roles">
                <mat-icon>admin_panel_settings</mat-icon>
              </button>
              
              <!-- Toggle Active/Inactive: SuperAdmin=all, Admin=non-SuperAdmin/non-Admin -->
              <button mat-icon-button [color]="u.isActive ? 'accent' : 'warn'" (click)="toggleStatus(u)" 
                *ngIf="canManageStatus(u)" matTooltip="{{u.isActive ? 'Deactivate' : 'Activate'}}">
                <mat-icon>{{u.isActive ? 'toggle_on' : 'toggle_off'}}</mat-icon>
              </button>

              <!-- Hard Delete: ONLY SuperAdmin -->
              <button mat-icon-button color="warn" (click)="deleteUser(u)" 
                *ngIf="canDelete(u)" matTooltip="Delete Permanently">
                <mat-icon>delete_forever</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols;"></tr>
        </table>
        <mat-paginator [length]="total" [pageSize]="ps" [pageIndex]="p-1" 
          [pageSizeOptions]="[5,10,25]" (page)="onPg($event)" *ngIf="total>5" showFirstLastButtons></mat-paginator>
      </div>

      <div class="empty" *ngIf="!loading && users.length===0">
        <mat-icon>people</mat-icon><h3>No users found</h3>
      </div>
    </div>

    <!-- Role Management Dialog -->
    <div class="dialog-overlay" *ngIf="showRoleDialog" (click)="closeDialog($event)">
      <div class="dialog-box" (click)="$event.stopPropagation()">
        <h2>Manage Roles - {{selectedUser?.firstName}} {{selectedUser?.lastName}}</h2>
        <p style="color:#666;margin-bottom:16px;">{{selectedUser?.email}}</p>
        
        <div style="margin-bottom:16px;">
          <p style="font-weight:500;margin-bottom:8px;">Available Roles:</p>
          <div style="display:flex;gap:8px;flex-wrap:wrap;">
            <mat-chip *ngFor="let role of availableRoles" 
              [class.selected]="selectedRoles.includes(role)"
              (click)="toggleRole(role)"
              [style.background-color]="selectedRoles.includes(role) ? roleColor(role) : '#f5f5f5'"
              [style.color]="selectedRoles.includes(role) ? '#fff' : '#666'"
              style="cursor:pointer;">
              {{role}}
            </mat-chip>
          </div>
          <p style="font-size:0.75rem;color:#999;margin-top:8px;">
            Click a role to assign or remove it
          </p>
        </div>

        <div class="dialog-btns">
          <button mat-stroked-button (click)="showRoleDialog=false">Cancel</button>
          <button mat-flat-button color="primary" (click)="saveRoles()" [disabled]="savingRoles">
            {{savingRoles ? 'Saving...' : 'Save Roles'}}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1400px;margin:0 auto}
    .header{display:flex;justify-content:space-between;align-items:center;margin-bottom:24px}
    .header h1{font-size:1.5rem;font-weight:700;margin:0}.header p{color:#666;font-size:.85rem}
    .filters{display:flex;align-items:center;gap:8px;margin-bottom:16px}.sf{width:320px}
    .table-wrap{background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.06);overflow:hidden}
    table{width:100%}th{background:#fafafa;font-weight:600;font-size:.78rem;text-transform:uppercase;color:#555;padding:14px 16px}td{padding:14px 16px;font-size:.88rem}
    tr:hover td{background:#f8f9ff}
    .role-chip{margin:2px;font-size:.72rem!important;height:24px!important}
    .badge{display:inline-block;padding:3px 12px;border-radius:20px;font-size:.75rem;font-weight:600}
    .badge.active{background:#e6f4ea;color:#137333}.badge.inactive{background:#f1f3f4;color:#5f6368}
    .empty{text-align:center;padding:64px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;color:#ccc}
    .dialog-overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1000;display:flex;align-items:center;justify-content:center}
    .dialog-box{background:#fff;border-radius:16px;padding:24px;width:90%;max-width:500px;box-shadow:0 20px 60px rgba(0,0,0,.3)}
    .dialog-box h2{margin:0 0 4px;font-size:1.2rem}
    .dialog-btns{display:flex;justify-content:flex-end;gap:12px;margin-top:20px}
    mat-form-field{width:100%}
    mat-chip.selected{font-weight:600}
    @media(max-width:768px){.page{padding:16px}.sf{width:100%}}
  `]
})
export class UserListComponent implements OnInit {
  private srv = inject(UserService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);

  users: any[] = [];
  total = 0;
  p = 1;
  ps = 10;
  sb = 'createdAt';
  sd: 'asc' | 'desc' = 'desc';
  searchText = '';
  statusFilter = '';
  loading = false;
  private search$ = new Subject<string>();

  showRoleDialog = false;
  selectedUser: any = null;
  selectedRoles: string[] = [];
  availableRoles: string[] = [];
  savingRoles = false;

  cols = ['name', 'roles', 'isActive', 'createdAt', 'actions'];

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300)).subscribe(() => { this.p = 1; this.fetch(); });
    this.fetch();
    this.setAvailableRoles();
  }

  setAvailableRoles(): void {
    const highestRole = this.auth.getHighestRole();
    if (highestRole === 'SuperAdmin') {
      this.availableRoles = ['SuperAdmin', 'Admin', 'Test', 'Viewer'];
    } else if (highestRole === 'Admin') {
      this.availableRoles = ['Test', 'Viewer'];
    } else {
      this.availableRoles = [];
    }
  }

  fetch(): void {
    this.loading = true;
    const params: any = { page: this.p, pageSize: this.ps, search: this.searchText || undefined, sortBy: this.sb, sortOrder: this.sd };
    if (this.statusFilter) params.isActive = this.statusFilter;
    this.srv.getUsers(params).subscribe({
      next: (r: any) => { if (r.success) { this.users = r.data; this.total = r.totalCount; } this.loading = false; },
      error: () => { this.loading = false; this.notify.error('Failed to load users'); }
    });
  }

  // ==================== RBAC CHECKS ====================

  /**
   * Manage Roles:
   * - SuperAdmin: can manage ALL users
   * - Admin: can manage Test and Viewer ONLY (not SuperAdmin, not Admin, not self)
   * - Test/Viewer: cannot manage anyone
   */
  canManageRoles(user: any): boolean {
    const currentHighestRole = this.auth.getHighestRole();
    if (currentHighestRole === 'SuperAdmin') return true;
    if (currentHighestRole === 'Admin') {
      const targetRoles = user.roles || [];
      if (targetRoles.includes('SuperAdmin') || targetRoles.includes('Admin')) return false;
      return true;
    }
    return false;
  }

  /**
   * Toggle Status (Activate/Deactivate):
   * - SuperAdmin: can toggle ALL users
   * - Admin: can toggle non-SuperAdmin/non-Admin users
   * - Test/Viewer: cannot toggle anyone
   */
  canManageStatus(user: any): boolean {
    const currentHighestRole = this.auth.getHighestRole();
    if (currentHighestRole === 'SuperAdmin') return true;
    if (currentHighestRole === 'Admin') {
      const targetRoles = user.roles || [];
      if (targetRoles.includes('SuperAdmin') || targetRoles.includes('Admin')) return false;
      return true;
    }
    return false;
  }

  /**
   * Hard Delete:
   * - ONLY SuperAdmin can delete
   * - Cannot delete yourself
   * - Admin/Test/Viewer: NO delete button
   */
  canDelete(user: any): boolean {
    const currentHighestRole = this.auth.getHighestRole();
    if (currentHighestRole !== 'SuperAdmin') return false;
    const currentUser = this.auth.getCurrentUser();
    if (currentUser && currentUser.id === user.id) return false;
    return true;
  }

  // ==================== ROLE DIALOG ====================

  openRoleDialog(user: any): void {
    this.selectedUser = user;
    this.selectedRoles = [...(user.roles || [])];
    this.showRoleDialog = true;
  }

  closeDialog(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('dialog-overlay')) {
      this.showRoleDialog = false;
    }
  }

  toggleRole(role: string): void {
    if (!this.availableRoles.includes(role)) {
      this.notify.warning(`You cannot assign the ${role} role`);
      return;
    }
    const idx = this.selectedRoles.indexOf(role);
    if (idx >= 0) { this.selectedRoles.splice(idx, 1); }
    else { this.selectedRoles.push(role); }
  }

  saveRoles(): void {
    if (!this.selectedUser) return;
    this.savingRoles = true;
    const validRoles = this.selectedRoles.filter(r => this.availableRoles.includes(r));
    this.srv.updateUserRoles(this.selectedUser.id, validRoles).subscribe({
      next: (r: any) => {
        this.savingRoles = false;
        if (r.success) { this.notify.success('Roles updated!'); this.showRoleDialog = false; this.fetch(); }
        else { this.notify.error(r.message || 'Failed'); }
      },
      error: (e: any) => { this.savingRoles = false; this.notify.error(e?.error?.message || 'Failed'); }
    });
  }

  // ==================== STATUS TOGGLE ====================

  async toggleStatus(user: any): Promise<void> {
    const action = user.isActive ? 'deactivate' : 'activate';
    const ok = await this.notify.confirmAction(
      `${user.isActive ? 'Deactivate' : 'Activate'} User`,
      `Are you sure you want to ${action} ${user.firstName} ${user.lastName}?`
    );
    if (!ok) return;
    this.srv.updateUserStatus(user.id, !user.isActive).subscribe({
      next: (r: any) => {
        if (r.success) { this.notify.success(`User ${user.isActive ? 'deactivated' : 'activated'}!`); this.fetch(); }
        else { this.notify.error(r.message || 'Failed'); }
      },
      error: (e: any) => { this.notify.error(e?.error?.message || 'Failed'); }
    });
  }

  // ==================== HARD DELETE (SuperAdmin only) ====================

  async deleteUser(user: any): Promise<void> {
    const ok = await this.notify.confirmDelete(`${user.firstName} ${user.lastName}`);
    if (!ok) return;
    this.srv.deleteUser(user.id).subscribe({
      next: (r: any) => {
        if (r.success) { this.notify.success('User deleted permanently!'); this.fetch(); }
        else { this.notify.error(r.message || 'Failed'); }
      },
      error: (e: any) => {
        if (e.status === 404) {
          this.srv.updateUserStatus(user.id, false).subscribe(() => {
            this.notify.warning('Delete endpoint not available. User deactivated instead.');
            this.fetch();
          });
        } else {
          this.notify.error(e?.error?.message || 'Failed');
        }
      }
    });
  }

  // ==================== HELPERS ====================

  roleColor(role: string): string {
    const c: Record<string, string> = { 'SuperAdmin': '#9c27b0', 'Admin': '#1976d2', 'Test': '#388e3c', 'Viewer': '#757575' };
    return c[role] || '#757575';
  }

  formatDate(d: string): string { return moment(d).format('DD/MM/YYYY'); }
  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }
  onSort(s: Sort): void { this.sb = s.active; this.sd = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPg(e: PageEvent): void { this.p = e.pageIndex + 1; this.ps = e.pageSize; this.fetch(); }
}