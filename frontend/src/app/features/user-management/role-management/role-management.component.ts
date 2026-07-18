import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { RoleService } from '../../../core/services/role.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-role-management',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule,
    MatCardModule, MatChipsModule, MatProgressBarModule, MatDividerModule,
    MatFormFieldModule, MatInputModule, MatCheckboxModule, MatTooltipModule,
    MatExpansionModule
  ],
  template: `
    <div class="page">
      <div class="header">
        <div>
          <h1>Role & Permission Management</h1>
          <p>Define roles and control access to every module</p>
        </div>
        <button mat-flat-button color="primary" (click)="openCreateDialog()" 
          *ngIf="auth.hasAnyRole(['SuperAdmin','Admin'])">
          <mat-icon>add</mat-icon> New Role
        </button>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>

      <div class="roles-grid" *ngIf="!loading">
        <mat-card *ngFor="let role of roles" class="role-card" [class.system]="role.isSystem">
          <mat-card-header>
            <mat-icon mat-card-avatar [style.color]="getRoleColor(role.name)">
              {{ role.isSystem ? 'lock' : 'badge' }}
            </mat-icon>
            <mat-card-title>
              {{ role.name }}
              <span class="system-badge" *ngIf="role.isSystem">System Role</span>
            </mat-card-title>
            <mat-card-subtitle>
              {{ role.description || 'No description' }} • {{ role.userCount || 0 }} users
            </mat-card-subtitle>
          </mat-card-header>
          <mat-divider></mat-divider>
          <mat-card-content>
            <p class="perm-label">Permissions ({{ role.permissions?.length || 0 }}/{{ totalPermissions }})</p>
            <div class="perm-chips">
              <mat-chip *ngFor="let perm of (role.permissions?.slice(0, 8) || [])" class="perm-chip">
                {{ perm }}
              </mat-chip>
              <mat-chip *ngIf="role.permissions?.length > 8" class="perm-chip more-chip" 
                (click)="showAllPermissions(role)" style="cursor:pointer;">
                +{{ role.permissions.length - 8 }} more
              </mat-chip>
              <span *ngIf="!role.permissions || role.permissions.length === 0" class="no-perms">No permissions</span>
            </div>
          </mat-card-content>
          <mat-divider></mat-divider>
          <mat-card-actions>
            <button mat-stroked-button color="primary" (click)="openEditDialog(role)"
              *ngIf="!role.isSystem || auth.hasRole('SuperAdmin')">
              <mat-icon>edit</mat-icon> Edit
            </button>
            <button mat-stroked-button color="warn" (click)="deleteRole(role)"
              *ngIf="!role.isSystem && auth.hasAnyRole(['SuperAdmin','Admin'])">
              <mat-icon>delete</mat-icon> Delete
            </button>
          </mat-card-actions>
        </mat-card>
      </div>

      <div class="empty" *ngIf="!loading && roles.length===0">
        <mat-icon>admin_panel_settings</mat-icon>
        <h3>No roles found</h3>
      </div>
    </div>

    <!-- ALL PERMISSIONS DIALOG -->
    <div class="dialog-overlay" *ngIf="showPermsDialog" (click)="showPermsDialog=false">
      <div class="dialog-box perms-dialog" (click)="$event.stopPropagation()">
        <div class="dialog-header">
          <h2>Permissions - {{ selectedRoleForPerms?.name }}</h2>
          <button mat-icon-button (click)="showPermsDialog=false"><mat-icon>close</mat-icon></button>
        </div>
        <mat-divider></mat-divider>
        <div class="dialog-body">
          <div class="all-perms">
            <mat-chip *ngFor="let perm of (selectedRoleForPerms?.permissions || [])" class="perm-chip">
              {{ perm }}
            </mat-chip>
          </div>
        </div>
      </div>
    </div>

    <!-- CREATE/EDIT ROLE DIALOG -->
    <div class="dialog-overlay" *ngIf="showDialog" (click)="closeDialog($event)">
      <div class="dialog-box role-dialog" (click)="$event.stopPropagation()">
        <div class="dialog-header">
          <h2>{{ editingRole ? 'Edit Role' : 'Create New Role' }}</h2>
          <button mat-icon-button (click)="showDialog=false"><mat-icon>close</mat-icon></button>
        </div>
        <mat-divider></mat-divider>
        <div class="dialog-body">
          <mat-form-field appearance="outline">
            <mat-label>Role Name *</mat-label>
            <input matInput [(ngModel)]="roleName" maxlength="50" placeholder="e.g., Cashier, Accountant">
            <mat-hint align="end">{{ roleName.length }}/50</mat-hint>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Description</mat-label>
            <input matInput [(ngModel)]="roleDescription" maxlength="200" placeholder="What this role can do">
          </mat-form-field>

          <h3 class="section-title">Module Permissions</h3>
          <p class="section-subtitle">Select which modules this role can access and what they can do.</p>

          <mat-form-field appearance="outline" class="search-modules">
            <mat-icon matPrefix>search</mat-icon>
            <input matInput [(ngModel)]="moduleSearch" placeholder="Search modules...">
          </mat-form-field>

          <div class="quick-actions">
            <button mat-button (click)="selectAll()">Select All</button>
            <button mat-button (click)="deselectAll()">Deselect All</button>
          </div>

          <div class="module-list">
            <div class="module-item" *ngFor="let mod of filteredModules">
              <div class="module-header">
                <mat-checkbox 
                  [checked]="isModuleFullySelected(mod)"
                  [indeterminate]="isModulePartiallySelected(mod)"
                  (change)="toggleModule(mod, $event.checked)">
                  <strong>{{ mod.moduleName }}</strong>
                </mat-checkbox>
              </div>
              <div class="perm-row" *ngIf="isModuleEnabled(mod)">
                <mat-checkbox *ngFor="let perm of mod.permissions" 
                  [checked]="isPermissionSelected(perm.id)"
                  (change)="togglePermission(perm.id)">
                  {{ formatPermName(perm.name) }}
                </mat-checkbox>
              </div>
              <mat-divider></mat-divider>
            </div>
          </div>

          <div class="summary">
            {{ selectedPermissionIds.size }} permissions selected
          </div>
        </div>
        <div class="dialog-actions">
          <button mat-stroked-button (click)="showDialog=false">Cancel</button>
          <button mat-flat-button color="primary" (click)="saveRole()" [disabled]="!roleName || saving">
            {{ saving ? 'Saving...' : (editingRole ? 'Update Role' : 'Create Role') }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1400px;margin:0 auto}
    .header{display:flex;justify-content:space-between;align-items:center;margin-bottom:24px}
    .header h1{font-size:1.5rem;font-weight:700;margin:0}.header p{color:#666;font-size:.85rem}
    .roles-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(380px,1fr));gap:20px}
    .role-card{border-radius:12px;box-shadow:0 2px 8px rgba(0,0,0,.06);border:1px solid #e8eaed}
    .role-card.system{border-left:4px solid #7c3aed}
    .role-card mat-card-header{padding:20px 20px 0}
    .role-card mat-card-content{padding:16px 20px 8px}
    .role-card mat-card-actions{padding:12px 20px;display:flex;gap:8px}
    mat-divider{margin:8px 0}
    .system-badge{font-size:.65rem;background:#f3e8ff;color:#7c3aed;padding:2px 8px;border-radius:10px;margin-left:8px;font-weight:600}
    .perm-label{font-weight:500;margin-bottom:8px;font-size:.85rem;color:#555}
    .perm-chips{display:flex;flex-wrap:wrap;gap:4px}
    .perm-chip{font-size:.68rem!important;height:22px!important;background:#e8f0fe!important;color:#1967d2!important}
    .more-chip{background:#f3e8ff!important;color:#7c3aed!important}
    .no-perms{color:#999;font-size:.85rem}
    .empty{text-align:center;padding:64px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;color:#ccc}

    .dialog-overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1000;display:flex;align-items:center;justify-content:center}
    .dialog-box{background:#fff;border-radius:16px;width:90%;max-width:750px;max-height:85vh;display:flex;flex-direction:column;box-shadow:0 20px 60px rgba(0,0,0,.3)}
    .perms-dialog{max-width:550px}
    .dialog-header{display:flex;justify-content:space-between;align-items:center;padding:20px 24px 0}
    .dialog-header h2{margin:0;font-size:1.2rem}
    .dialog-body{padding:16px 24px;overflow-y:auto;flex:1}
    .dialog-actions{display:flex;justify-content:flex-end;gap:12px;padding:16px 24px;border-top:1px solid #eee}
    .section-title{font-size:.9rem;font-weight:600;color:#333;margin:16px 0 4px}
    .section-subtitle{font-size:.8rem;color:#888;margin:0 0 12px}
    .search-modules{width:100%}
    .quick-actions{display:flex;gap:8px;margin-bottom:12px}
    .module-list{max-height:400px;overflow-y:auto}
    .module-item{padding:8px 0}
    .module-header{margin-bottom:4px}
    .perm-row{display:flex;flex-wrap:wrap;gap:12px;padding-left:32px;margin-bottom:8px}
    .summary{text-align:center;padding:12px;background:#f0f4ff;border-radius:8px;font-weight:500;font-size:.85rem;color:#1a73e8;margin-top:12px}
    .all-perms{display:flex;flex-wrap:wrap;gap:6px;padding:8px 0}
    mat-form-field{width:100%}
    @media(max-width:768px){.page{padding:16px}.roles-grid{grid-template-columns:1fr}.dialog-box{width:95vw;max-height:90vh}.perm-row{flex-direction:column;gap:4px}}
  `]
})
export class RoleManagementComponent implements OnInit {
  private roleSrv = inject(RoleService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);

  roles: any[] = [];
  allModules: any[] = [];
  totalPermissions = 0;
  loading = false;

  showDialog = false;
  editingRole: any = null;
  roleName = '';
  roleDescription = '';
  selectedPermissionIds = new Set<string>();
  moduleSearch = '';
  saving = false;

  showPermsDialog = false;
  selectedRoleForPerms: any = null;

  ngOnInit(): void {
    if (!this.auth.hasAnyRole(['SuperAdmin', 'Admin'])) {
      this.notify.error('Access denied');
      return;
    }
    this.fetch();
    this.loadModules();
  }

  get filteredModules(): any[] {
    if (!this.moduleSearch) return this.allModules;
    const search = this.moduleSearch.toLowerCase();
    return this.allModules.filter(m => m.moduleName.toLowerCase().includes(search));
  }

  fetch(): void {
    this.loading = true;
    this.roleSrv.getAll().subscribe({
      next: (r: any) => { if (r.success) { this.roles = r.data; this.totalPermissions = this.calcTotalPerms(r.data); } this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  calcTotalPerms(roles: any[]): number {
    const allPerms = new Set<string>();
    roles.forEach(r => r.permissions?.forEach((p: string) => allPerms.add(p)));
    return allPerms.size;
  }

  loadModules(): void {
    this.roleSrv.getModulePermissions().subscribe({
      next: (r: any) => { if (r.success) this.allModules = r.data || []; }
    });
  }

  showAllPermissions(role: any): void {
    this.selectedRoleForPerms = role;
    this.showPermsDialog = true;
  }

  openCreateDialog(): void {
    this.editingRole = null;
    this.roleName = '';
    this.roleDescription = '';
    this.selectedPermissionIds.clear();
    this.moduleSearch = '';
    this.showDialog = true;
  }

  openEditDialog(role: any): void {
    this.editingRole = role;
    this.roleName = role.name || '';
    this.roleDescription = role.description || '';
    this.selectedPermissionIds = new Set<string>();
    this.moduleSearch = '';
    if (role.permissions) {
      this.allModules.forEach(mod => {
        mod.permissions?.forEach((perm: any) => {
          if (role.permissions.includes(perm.name)) {
            this.selectedPermissionIds.add(perm.id);
          }
        });
      });
    }
    this.showDialog = true;
  }

  closeDialog(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('dialog-overlay')) {
      this.showDialog = false;
    }
  }

  isModuleEnabled(mod: any): boolean {
    return mod.permissions?.some((p: any) => this.selectedPermissionIds.has(p.id));
  }

  isModuleFullySelected(mod: any): boolean {
    if (!mod.permissions?.length) return false;
    return mod.permissions.every((p: any) => this.selectedPermissionIds.has(p.id));
  }

  isModulePartiallySelected(mod: any): boolean {
    if (!mod.permissions?.length) return false;
    const selected = mod.permissions.filter((p: any) => this.selectedPermissionIds.has(p.id)).length;
    return selected > 0 && selected < mod.permissions.length;
  }

  isPermissionSelected(permId: string): boolean { return this.selectedPermissionIds.has(permId); }

  toggleModule(mod: any, checked: boolean): void {
    mod.permissions?.forEach((p: any) => {
      if (checked) this.selectedPermissionIds.add(p.id);
      else this.selectedPermissionIds.delete(p.id);
    });
  }

  togglePermission(permId: string): void {
    if (this.selectedPermissionIds.has(permId)) this.selectedPermissionIds.delete(permId);
    else this.selectedPermissionIds.add(permId);
  }

  selectAll(): void {
    this.allModules.forEach(mod => {
      mod.permissions?.forEach((p: any) => this.selectedPermissionIds.add(p.id));
    });
  }

  deselectAll(): void { this.selectedPermissionIds.clear(); }

  saveRole(): void {
    if (!this.roleName.trim()) return;
    this.saving = true;
    const data = {
      name: this.roleName.trim(),
      description: this.roleDescription.trim(),
      permissionIds: Array.from(this.selectedPermissionIds)
    };

    const request = this.editingRole
      ? this.roleSrv.update(this.editingRole.id, data)
      : this.roleSrv.create(data);

    request.subscribe({
      next: (r: any) => {
        this.saving = false;
        if (r.success) {
          this.notify.success(this.editingRole ? 'Role updated!' : 'Role created!');
          this.showDialog = false;
          this.fetch();
        } else {
          this.notify.error(r.message || 'Failed');
        }
      },
      error: (e: any) => {
        this.saving = false;
        this.notify.error(e?.error?.message || 'Failed');
      }
    });
  }

  async deleteRole(role: any): Promise<void> {
    if (role.isSystem) { this.notify.error('System roles cannot be deleted'); return; }
    const ok = await this.notify.confirmAction('Delete Role', `Delete "${role.name}"? This cannot be undone.`);
    if (!ok) return;
    this.roleSrv.delete(role.id).subscribe({
      next: (r: any) => {
        if (r.success) { this.notify.success('Role deleted!'); this.fetch(); }
        else { this.notify.error(r.message || 'Failed'); }
      },
      error: (e: any) => { this.notify.error(e?.error?.message || 'Failed'); }
    });
  }

  formatPermName(name: string): string {
    return name.replace(/([A-Z])/g, ' $1').replace(/\./g, ' ').trim();
  }

  getRoleColor(role: string): string {
    const c: Record<string, string> = { 'SuperAdmin': '#9c27b0', 'Admin': '#1976d2', 'Test': '#388e3c', 'Viewer': '#757575' };
    return c[role] || '#f57c00';
  }
}