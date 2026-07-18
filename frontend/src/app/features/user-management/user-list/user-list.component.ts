import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { UserService } from '../../../core/services/user.service';
import { RoleService } from '../../../core/services/role.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule,
    MatTableModule, MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatProgressBarModule, MatTooltipModule, MatCardModule, MatChipsModule,
    MatDividerModule, MatSlideToggleModule, MatCheckboxModule
  ],
  template: `
    <div class="page">
      <div class="header">
        <div><h1>User Management</h1><p>{{total}} users total</p></div>
        <button mat-flat-button color="primary" (click)="openCreateUserDialog()" 
          *ngIf="auth.hasAnyRole(['SuperAdmin','Admin'])">
          <mat-icon>add</mat-icon> New User
        </button>
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
        <mat-form-field appearance="outline" style="width:180px">
          <mat-label>Role</mat-label>
          <mat-select [(ngModel)]="roleFilter" (selectionChange)="fetch()">
            <mat-option value="">All Roles</mat-option>
            <mat-option *ngFor="let role of allRoles" [value]="role.name">{{ role.name }}</mat-option>
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
              <button mat-icon-button color="primary" (click)="openRoleDialog(u)" 
                *ngIf="canManageRoles(u)" matTooltip="Manage Roles">
                <mat-icon>admin_panel_settings</mat-icon>
              </button>
              <button mat-icon-button [color]="u.isActive ? 'accent' : 'warn'" (click)="toggleStatus(u)" 
                *ngIf="canManageStatus(u)" matTooltip="{{u.isActive ? 'Deactivate' : 'Activate'}}">
                <mat-icon>{{u.isActive ? 'toggle_on' : 'toggle_off'}}</mat-icon>
              </button>
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

    <!-- CREATE USER DIALOG -->
    <div class="dialog-overlay" *ngIf="showCreateDialog" (click)="closeCreateDialog($event)">
      <div class="dialog-box" (click)="$event.stopPropagation()">
        <div class="dialog-header"><h2>Create New User</h2><button mat-icon-button (click)="showCreateDialog=false"><mat-icon>close</mat-icon></button></div>
        <mat-divider></mat-divider>
        <div class="dialog-body">
          <div class="g2">
            <mat-form-field appearance="outline"><mat-label>First Name *</mat-label><input matInput [(ngModel)]="newUser.firstName" required></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Last Name *</mat-label><input matInput [(ngModel)]="newUser.lastName" required></mat-form-field>
          </div>
          <div class="g2">
            <mat-form-field appearance="outline"><mat-label>Email *</mat-label><input matInput [(ngModel)]="newUser.email" type="email" required></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Phone</mat-label><input matInput [(ngModel)]="newUser.phone"></mat-form-field>
          </div>
          <div class="g2">
            <mat-form-field appearance="outline"><mat-label>Password *</mat-label><input matInput [(ngModel)]="newUser.password" [type]="hidePass ? 'password' : 'text'"><button mat-icon-button matSuffix (click)="hidePass=!hidePass" type="button"><mat-icon>{{hidePass ? 'visibility_off' : 'visibility'}}</mat-icon></button><mat-hint>Min 8 chars, 1 uppercase, 1 number, 1 symbol</mat-hint></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Confirm Password *</mat-label><input matInput [(ngModel)]="newUser.confirmPassword" [type]="hidePass ? 'password' : 'text'"></mat-form-field>
          </div>
          <mat-form-field appearance="outline"><mat-label>Role *</mat-label><mat-select [(ngModel)]="newUser.roleId" (selectionChange)="onRoleSelect()"><mat-option *ngFor="let role of availableRolesForUser" [value]="role.id">{{ role.name }} {{ role.isSystem ? '(System)' : '(Custom)' }}</mat-option></mat-select></mat-form-field>
          <div class="role-preview" *ngIf="selectedRolePreview"><p class="preview-title">Permissions for: {{ selectedRolePreview.name }}</p><div class="preview-perms"><mat-chip *ngFor="let perm of (selectedRolePreview.permissions?.slice(0, 8) || [])" class="preview-chip">{{ formatPermName(perm) }}</mat-chip><span *ngIf="selectedRolePreview.permissions?.length > 8" class="more-perms">+{{ selectedRolePreview.permissions.length - 8 }} more</span><span *ngIf="!selectedRolePreview.permissions?.length" style="color:#999;font-size:0.8rem;">No permissions</span></div></div>
          <mat-slide-toggle [(ngModel)]="newUser.isActive" color="primary" style="margin-top:8px;">Active (User can login immediately)</mat-slide-toggle>
        </div>
        <div class="dialog-actions"><button mat-stroked-button (click)="showCreateDialog=false">Cancel</button><button mat-flat-button color="primary" (click)="createUser()" [disabled]="creatingUser">{{ creatingUser ? 'Creating...' : 'Create User' }}</button></div>
      </div>
    </div>

    <!-- MANAGE ROLES DIALOG -->
    <div class="dialog-overlay" *ngIf="showRoleDialog" (click)="closeDialog($event)">
      <div class="dialog-box" (click)="$event.stopPropagation()">
        <div class="dialog-header"><h2>Manage Roles - {{selectedUser?.firstName}} {{selectedUser?.lastName}}</h2><button mat-icon-button (click)="showRoleDialog=false"><mat-icon>close</mat-icon></button></div>
        <mat-divider></mat-divider>
        <div class="dialog-body">
          <p style="color:#666;margin-bottom:16px;">{{selectedUser?.email}}</p>
          <div style="margin-bottom:16px;">
            <p style="font-weight:500;margin-bottom:8px;">Available Roles:</p>
            <div style="display:flex;gap:8px;flex-wrap:wrap;">
              <mat-chip *ngFor="let role of availableRoles" [class.selected]="selectedRoles.includes(role)" (click)="toggleRole(role)" [style.background-color]="selectedRoles.includes(role) ? roleColor(role) : '#f5f5f5'" [style.color]="selectedRoles.includes(role) ? '#fff' : '#666'" style="cursor:pointer;">{{role}}</mat-chip>
            </div>
            <p style="font-size:0.75rem;color:#999;margin-top:8px;">Click a role to assign or remove it. Green = assigned.</p>
          </div>
          <button mat-stroked-button color="primary" (click)="openCreateRoleInline()" style="margin-bottom:16px;width:100%;"><mat-icon>add</mat-icon> Create New Role</button>
        </div>
        <div class="dialog-actions"><button mat-stroked-button (click)="showRoleDialog=false">Cancel</button><button mat-flat-button color="primary" (click)="saveRoles()" [disabled]="savingRoles">{{savingRoles ? 'Saving...' : 'Save Roles'}}</button></div>
      </div>
    </div>

    <!-- INLINE CREATE ROLE DIALOG -->
    <div class="dialog-overlay" *ngIf="showInlineRoleDialog" (click)="showInlineRoleDialog=false">
      <div class="dialog-box" (click)="$event.stopPropagation()">
        <div class="dialog-header"><h2>Create New Role</h2><button mat-icon-button (click)="showInlineRoleDialog=false"><mat-icon>close</mat-icon></button></div>
        <mat-divider></mat-divider>
        <div class="dialog-body">
          <mat-form-field appearance="outline"><mat-label>Role Name *</mat-label><input matInput [(ngModel)]="inlineRoleName" maxlength="50" placeholder="e.g., Cashier"></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Description</mat-label><input matInput [(ngModel)]="inlineRoleDesc" maxlength="200" placeholder="What this role can do"></mat-form-field>
          <h3 class="section-title">Module Permissions</h3>
          <div class="module-list">
            <div class="module-item" *ngFor="let mod of allModules">
              <div class="module-header"><mat-checkbox [checked]="isInlineModuleFullySelected(mod)" [indeterminate]="isInlineModulePartiallySelected(mod)" (change)="toggleInlineModule(mod, $event.checked)"><strong>{{ mod.moduleName }}</strong></mat-checkbox></div>
              <div class="perm-row" *ngIf="isInlineModuleEnabled(mod)"><mat-checkbox *ngFor="let perm of mod.permissions" [checked]="inlineSelectedPerms.has(perm.id)" (change)="toggleInlinePerm(perm.id)">{{ formatPermName(perm.name) }}</mat-checkbox></div>
              <mat-divider></mat-divider>
            </div>
          </div>
          <div class="summary">{{ inlineSelectedPerms.size }} permissions selected</div>
        </div>
        <div class="dialog-actions"><button mat-stroked-button (click)="showInlineRoleDialog=false">Cancel</button><button mat-flat-button color="primary" (click)="saveInlineRole()" [disabled]="!inlineRoleName || savingInlineRole">{{ savingInlineRole ? 'Creating...' : 'Create Role' }}</button></div>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1400px;margin:0 auto}
    .header{display:flex;justify-content:space-between;align-items:center;margin-bottom:24px}
    .header h1{font-size:1.5rem;font-weight:700;margin:0}.header p{color:#666;font-size:.85rem}
    .filters{display:flex;align-items:center;gap:8px;margin-bottom:16px;flex-wrap:wrap}.sf{width:320px}
    .table-wrap{background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.06);overflow:hidden}
    table{width:100%}th{background:#fafafa;font-weight:600;font-size:.78rem;text-transform:uppercase;color:#555;padding:14px 16px}td{padding:14px 16px;font-size:.88rem}
    tr:hover td{background:#f8f9ff}
    .role-chip{margin:2px;font-size:.72rem!important;height:24px!important}
    .badge{display:inline-block;padding:3px 12px;border-radius:20px;font-size:.75rem;font-weight:600}
    .badge.active{background:#e6f4ea;color:#137333}.badge.inactive{background:#f1f3f4;color:#5f6368}
    .empty{text-align:center;padding:64px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;color:#ccc}
    .dialog-overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1000;display:flex;align-items:center;justify-content:center}
    .dialog-box{background:#fff;border-radius:16px;width:90%;max-width:650px;max-height:85vh;display:flex;flex-direction:column;box-shadow:0 20px 60px rgba(0,0,0,.3)}
    .dialog-header{display:flex;justify-content:space-between;align-items:center;padding:20px 24px 0}
    .dialog-header h2{margin:0;font-size:1.2rem}
    .dialog-body{padding:16px 24px;overflow-y:auto;flex:1}
    .dialog-actions{display:flex;justify-content:flex-end;gap:12px;padding:16px 24px;border-top:1px solid #eee}
    .g2{display:grid;grid-template-columns:1fr 1fr;gap:12px}
    mat-form-field{width:100%}
    mat-chip.selected{font-weight:600}
    .role-preview{background:#f0f4ff;border-radius:8px;padding:12px;margin-top:12px}
    .preview-title{font-weight:600;font-size:.85rem;color:#1a73e8;margin:0 0 8px}
    .preview-perms{display:flex;flex-wrap:wrap;gap:4px;align-items:center}
    .preview-chip{font-size:.65rem!important;height:20px!important;background:#e8f0fe!important;color:#1967d2!important}
    .more-perms{font-size:.72rem;color:#7c3aed;font-weight:500}
    .section-title{font-size:.9rem;font-weight:600;color:#333;margin:16px 0 4px}
    .module-list{max-height:300px;overflow-y:auto}
    .module-item{padding:8px 0}
    .module-header{margin-bottom:4px}
    .perm-row{display:flex;flex-wrap:wrap;gap:12px;padding-left:32px;margin-bottom:8px}
    .summary{text-align:center;padding:8px;background:#f0f4ff;border-radius:8px;font-weight:500;font-size:.85rem;color:#1a73e8;margin-top:8px}
    @media(max-width:768px){.page{padding:16px}.sf{width:100%}.g2{grid-template-columns:1fr}.dialog-box{width:95vw}}
  `]
})
export class UserListComponent implements OnInit {
  private srv = inject(UserService);
  private roleSrv = inject(RoleService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);

  users: any[] = []; total = 0; p = 1; ps = 10; sb = 'createdAt'; sd: 'asc' | 'desc' = 'desc';
  searchText = ''; statusFilter = ''; roleFilter = '';
  loading = false;
  private search$ = new Subject<string>();

  showRoleDialog = false; selectedUser: any = null;
  selectedRoles: string[] = []; availableRoles: string[] = []; savingRoles = false;

  showCreateDialog = false; creatingUser = false; hidePass = true;
  newUser: any = { firstName: '', lastName: '', email: '', phone: '', password: '', confirmPassword: '', roleId: '', isActive: true };
  allRoles: any[] = []; selectedRolePreview: any = null;

  showInlineRoleDialog = false; inlineRoleName = ''; inlineRoleDesc = '';
  inlineSelectedPerms = new Set<string>(); savingInlineRole = false;
  allModules: any[] = [];

  cols = ['name', 'roles', 'isActive', 'createdAt', 'actions'];

  get availableRolesForUser(): any[] {
    const hr = this.auth.getHighestRole();
    if (hr === 'SuperAdmin') return this.allRoles;
    if (hr === 'Admin') return this.allRoles.filter(r => !['SuperAdmin', 'Admin'].includes(r.name));
    return [];
  }

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300)).subscribe(() => { this.p = 1; this.fetch(); });
    this.loadAllRoles();
    this.loadModules();
    this.fetch();
  }

  loadAllRoles(): void { this.roleSrv.getAll().subscribe({ next: (r: any) => { if (r.success) { this.allRoles = r.data || []; this.setAvailableRoles(); } } }); }

  loadModules(): void { this.roleSrv.getModulePermissions().subscribe({ next: (r: any) => { if (r.success) this.allModules = r.data || []; } }); }

  setAvailableRoles(): void {
    const hr = this.auth.getHighestRole();
    // SuperAdmin can assign ALL roles including custom ones
    if (hr === 'SuperAdmin') {
      this.availableRoles = this.allRoles.map(r => r.name);
    } 
    // Admin can assign all EXCEPT SuperAdmin and Admin
    else if (hr === 'Admin') {
      this.availableRoles = this.allRoles
        .filter(r => r.name !== 'SuperAdmin' && r.name !== 'Admin')
        .map(r => r.name);
    } else {
      this.availableRoles = [];
    }
  }

  fetch(): void {
    this.loading = true;
    const params: any = { page: this.p, pageSize: this.ps, search: this.searchText || undefined, sortBy: this.sb, sortOrder: this.sd };
    if (this.statusFilter) params.isActive = this.statusFilter;
    if (this.roleFilter) params.role = this.roleFilter;
    this.srv.getUsers(params).subscribe({ next: (r: any) => { if (r.success) { this.users = r.data; this.total = r.totalCount; } this.loading = false; }, error: () => { this.loading = false; } });
  }

  openCreateUserDialog(): void { this.newUser = { firstName: '', lastName: '', email: '', phone: '', password: '', confirmPassword: '', roleId: '', isActive: true }; this.selectedRolePreview = null; this.showCreateDialog = true; }
  closeCreateDialog(event: MouseEvent): void { if ((event.target as HTMLElement).classList.contains('dialog-overlay')) this.showCreateDialog = false; }
  onRoleSelect(): void { const role = this.allRoles.find(r => r.id === this.newUser.roleId); this.selectedRolePreview = role || null; }

  createUser(): void {
    if (!this.newUser.firstName || !this.newUser.lastName || !this.newUser.email || !this.newUser.password || !this.newUser.roleId) { this.notify.error('Please fill all required fields'); return; }
    if (this.newUser.password !== this.newUser.confirmPassword) { this.notify.error('Passwords do not match'); return; }
    if (this.newUser.password.length < 8) { this.notify.error('Password must be at least 8 characters'); return; }

    this.creatingUser = true;
    // Use the register endpoint since POST /users returns 405
    this.srv.createUser({
      firstName: this.newUser.firstName,
      lastName: this.newUser.lastName,
      email: this.newUser.email,
      phoneNumber: this.newUser.phone,
      password: this.newUser.password,
      confirmPassword: this.newUser.confirmPassword,
      roleId: this.newUser.roleId,
      isActive: this.newUser.isActive
    }).subscribe({
      next: (r: any) => {
        this.creatingUser = false;
        if (r.success) {
          this.notify.success('User created successfully!');
          this.showCreateDialog = false;
          this.fetch();
        } else {
          this.notify.error(r.message || 'Failed to create user');
        }
      },
      error: (e: any) => {
        this.creatingUser = false;
        // If 405, try alternative endpoint
        if (e.status === 405) {
          this.notify.error('Create user endpoint not available. Please use the Register page or contact administrator.');
        } else {
          this.notify.error(e?.error?.message || 'Failed to create user');
        }
      }
    });
  }

  openCreateRoleInline(): void { this.inlineRoleName = ''; this.inlineRoleDesc = ''; this.inlineSelectedPerms.clear(); this.showInlineRoleDialog = true; }

  isInlineModuleEnabled(mod: any): boolean { return mod.permissions?.some((p: any) => this.inlineSelectedPerms.has(p.id)); }
  isInlineModuleFullySelected(mod: any): boolean { if (!mod.permissions?.length) return false; return mod.permissions.every((p: any) => this.inlineSelectedPerms.has(p.id)); }
  isInlineModulePartiallySelected(mod: any): boolean { if (!mod.permissions?.length) return false; const s = mod.permissions.filter((p: any) => this.inlineSelectedPerms.has(p.id)).length; return s > 0 && s < mod.permissions.length; }
  toggleInlineModule(mod: any, checked: boolean): void { mod.permissions?.forEach((p: any) => { if (checked) this.inlineSelectedPerms.add(p.id); else this.inlineSelectedPerms.delete(p.id); }); }
  toggleInlinePerm(permId: string): void { if (this.inlineSelectedPerms.has(permId)) this.inlineSelectedPerms.delete(permId); else this.inlineSelectedPerms.add(permId); }

  saveInlineRole(): void {
    if (!this.inlineRoleName.trim()) return;
    this.savingInlineRole = true;
    this.roleSrv.create({ name: this.inlineRoleName.trim(), description: this.inlineRoleDesc.trim(), permissionIds: Array.from(this.inlineSelectedPerms) }).subscribe({
      next: (r: any) => {
        this.savingInlineRole = false;
        if (r.success) {
          this.notify.success('Role created!');
          this.showInlineRoleDialog = false;
          this.loadAllRoles();
        } else this.notify.error(r.message || 'Failed');
      },
      error: (e: any) => { this.savingInlineRole = false; this.notify.error(e?.error?.message || 'Failed'); }
    });
  }

  canManageRoles(user: any): boolean { const hr = this.auth.getHighestRole(); if (hr === 'SuperAdmin') return true; if (hr === 'Admin') { const tr = user.roles || []; if (tr.includes('SuperAdmin') || tr.includes('Admin')) return false; return true; } return false; }
  canManageStatus(user: any): boolean { const hr = this.auth.getHighestRole(); if (hr === 'SuperAdmin') return true; if (hr === 'Admin') { const tr = user.roles || []; if (tr.includes('SuperAdmin') || tr.includes('Admin')) return false; return true; } return false; }
  canDelete(user: any): boolean { const hr = this.auth.getHighestRole(); if (hr !== 'SuperAdmin') return false; const cu = this.auth.getCurrentUser(); if (cu && cu.id === user.id) return false; return true; }

  openRoleDialog(user: any): void { this.selectedUser = user; this.selectedRoles = [...(user.roles || [])]; this.setAvailableRoles(); this.showRoleDialog = true; }
  closeDialog(event: MouseEvent): void { if ((event.target as HTMLElement).classList.contains('dialog-overlay')) this.showRoleDialog = false; }

  // FIXED: toggleRole now accepts ALL roles in availableRoles list (including custom roles)
  toggleRole(role: string): void {
    const idx = this.selectedRoles.indexOf(role);
    if (idx >= 0) {
      this.selectedRoles.splice(idx, 1);
    } else {
      // Check if this role is in the available roles list
      if (this.availableRoles.includes(role)) {
        this.selectedRoles.push(role);
      } else {
        this.notify.warning(`You cannot assign the role: ${role}`);
      }
    }
  }

  saveRoles(): void {
    if (!this.selectedUser) return;
    this.savingRoles = true;
    // Only send roles that are in the available list
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

  async toggleStatus(user: any): Promise<void> { const action = user.isActive ? 'deactivate' : 'activate'; const ok = await this.notify.confirmAction(`${user.isActive ? 'Deactivate' : 'Activate'} User`, `Are you sure you want to ${action} ${user.firstName} ${user.lastName}?`); if (!ok) return; this.srv.updateUserStatus(user.id, !user.isActive).subscribe({ next: (r: any) => { if (r.success) { this.notify.success(`User ${user.isActive ? 'deactivated' : 'activated'}!`); this.fetch(); } else this.notify.error(r.message || 'Failed'); }, error: (e: any) => { this.notify.error(e?.error?.message || 'Failed'); } }); }
  async deleteUser(user: any): Promise<void> { const ok = await this.notify.confirmAction('Delete User Permanently', `Permanently delete ${user.firstName} ${user.lastName}?`); if (!ok) return; this.srv.deleteUser(user.id).subscribe({ next: (r: any) => { if (r.success) { this.notify.success('User deleted!'); this.fetch(); } else this.notify.error(r.message || 'Failed'); }, error: (e: any) => { if (e.status === 404) { this.srv.updateUserStatus(user.id, false).subscribe(() => { this.notify.warning('User deactivated instead.'); this.fetch(); }); } else this.notify.error(e?.error?.message || 'Failed'); } }); }

  roleColor(role: string): string { const c: Record<string, string> = { 'SuperAdmin': '#9c27b0', 'Admin': '#1976d2', 'Test': '#388e3c', 'Viewer': '#757575' }; return c[role] || '#f57c00'; }
  formatPermName(name: string): string { return name.replace(/([A-Z])/g, ' $1').replace(/\./g, ' ').trim(); }
  formatDate(d: string): string { return moment(d).format('DD/MM/YYYY'); }
  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }
  onSort(s: Sort): void { this.sb = s.active; this.sd = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPg(e: PageEvent): void { this.p = e.pageIndex + 1; this.ps = e.pageSize; this.fetch(); }
}