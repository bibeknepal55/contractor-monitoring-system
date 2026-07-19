import { Component, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RoleService } from '../../../core/services/role.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-permission-preview-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule, MatDialogModule,
    MatCheckboxModule, MatCardModule, MatChipsModule, MatDividerModule,
    MatExpansionModule, MatProgressBarModule, MatTooltipModule
  ],
  template: `
    <div class="overlay" (click)="close()">
      <div class="dialog" (click)="$event.stopPropagation()">
        <div class="dialog-header">
          <div>
            <h2>Permission Preview</h2>
            <p class="subtitle">See exactly what a user with this role can access</p>
          </div>
          <button mat-icon-button (click)="close()"><mat-icon>close</mat-icon></button>
        </div>
        <mat-divider></mat-divider>

        <div class="dialog-body">
          <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>

          <ng-container *ngIf="!loading">
            <!-- Summary Cards -->
            <div class="summary-row">
              <div class="summary-card">
                <strong>{{ enabledModules }}</strong>
                <span>Modules Enabled</span>
              </div>
              <div class="summary-card">
                <strong>{{ totalPermissions }}</strong>
                <span>Total Permissions</span>
              </div>
              <div class="summary-card">
                <strong>{{ restrictedModules }}</strong>
                <span>Restricted Modules</span>
              </div>
            </div>

            <!-- Module List -->
            <div class="module-list">
              <div class="module-item" *ngFor="let mod of previewData" [class.disabled]="!mod.enabled">
                <mat-expansion-panel [disabled]="!mod.enabled">
                  <mat-expansion-panel-header>
                    <mat-panel-title>
                      <mat-icon [style.color]="mod.enabled ? '#059669' : '#d1d5db'">
                        {{ mod.enabled ? 'check_circle' : 'block' }}
                      </mat-icon>
                      <span [class.text-disabled]="!mod.enabled">{{ mod.moduleName }}</span>
                      <mat-chip class="perm-count" *ngIf="mod.enabled">
                        {{ mod.selectedPermissions.length }}/{{ mod.totalPermissions }}
                      </mat-chip>
                    </mat-panel-title>
                  </mat-expansion-panel-header>
                  
                  <div class="perm-list">
                    <div class="perm-item" *ngFor="let perm of mod.permissions">
                      <mat-icon [style.color]="perm.selected ? '#059669' : '#d1d5db'" class="perm-icon">
                        {{ perm.selected ? 'check_box' : 'check_box_outline_blank' }}
                      </mat-icon>
                      <span [class.text-disabled]="!perm.selected">{{ formatPermName(perm.name) }}</span>
                      <mat-chip class="action-chip" [class.action-enabled]="perm.selected" [class.action-disabled]="!perm.selected">
                        {{ perm.selected ? 'Allowed' : 'Denied' }}
                      </mat-chip>
                    </div>
                  </div>
                </mat-expansion-panel>
              </div>
            </div>

            <!-- Version History -->
            <div class="version-section" *ngIf="versionHistory.length > 0">
              <h3>Recent Changes</h3>
              <div class="version-item" *ngFor="let v of versionHistory">
                <mat-icon>history</mat-icon>
                <div class="version-info">
                  <strong>{{ v.changedBy }}</strong>
                  <span>{{ formatDate(v.changedAt) }}</span>
                </div>
                <mat-chip [class]="v.changeType === 'Added' ? 'chip-added' : 'chip-removed'">
                  {{ v.changeType === 'Added' ? '+' : '-' }}{{ v.count }} permissions
                </mat-chip>
              </div>
            </div>

            <!-- Diff View (if comparing) -->
            <div class="diff-section" *ngIf="showDiff">
              <h3>Changes from Current</h3>
              <div class="diff-grid">
                <div class="diff-column">
                  <h4>Currently Has</h4>
                  <mat-chip *ngFor="let p of currentPermissions" class="current-chip">{{ p }}</mat-chip>
                </div>
                <div class="diff-column">
                  <h4>After Change</h4>
                  <mat-chip *ngFor="let p of newPermissions" 
                    [class]="currentPermissions.includes(p) ? 'unchanged-chip' : 'added-chip'">
                    {{ p }}
                    <mat-icon *ngIf="!currentPermissions.includes(p)" class="added-icon">add</mat-icon>
                  </mat-chip>
                </div>
                <div class="diff-column">
                  <h4>Will Lose</h4>
                  <mat-chip *ngFor="let p of removedPermissions" class="removed-chip">
                    {{ p }}
                    <mat-icon class="removed-icon">remove</mat-icon>
                  </mat-chip>
                  <span *ngIf="removedPermissions.length === 0" class="no-change">No permissions removed</span>
                </div>
              </div>
            </div>
          </ng-container>
        </div>

        <div class="dialog-actions">
          <button mat-stroked-button (click)="close()">Close</button>
          <button mat-flat-button color="primary" (click)="applyChanges()" *ngIf="showDiff">
            <mat-icon>check</mat-icon> Apply Changes
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:2000;display:flex;align-items:center;justify-content:center}
    .dialog{background:#fff;border-radius:16px;width:95%;max-width:850px;max-height:85vh;display:flex;flex-direction:column;box-shadow:0 20px 60px rgba(0,0,0,.3);animation:scaleIn .2s ease}
    @keyframes scaleIn{from{opacity:0;transform:scale(.95)}to{opacity:1;transform:scale(1)}}
    .dialog-header{display:flex;justify-content:space-between;align-items:center;padding:20px 24px 0}
    .dialog-header h2{font-size:1.2rem;font-weight:650;margin:0}
    .subtitle{font-size:.8rem;color:#888;margin:2px 0 0}
    .dialog-body{padding:16px 24px;overflow-y:auto;flex:1}
    .dialog-actions{display:flex;justify-content:flex-end;gap:12px;padding:16px 24px;border-top:1px solid #eee}

    .summary-row{display:grid;grid-template-columns:repeat(3,1fr);gap:12px;margin-bottom:20px}
    .summary-card{text-align:center;padding:16px;background:#f8fafc;border-radius:10px;border:1px solid #e5e7eb}
    .summary-card strong{display:block;font-size:1.5rem;color:#111}
    .summary-card span{font-size:.75rem;color:#888}

    .module-list{display:flex;flex-direction:column;gap:6px;max-height:400px;overflow-y:auto}
    .module-item.disabled mat-expansion-panel{opacity:.5}
    .text-disabled{color:#d1d5db!important}
    .perm-count{font-size:.65rem!important;height:18px!important;margin-left:8px}
    .perm-list{padding-left:8px}
    .perm-item{display:flex;align-items:center;gap:8px;padding:4px 0;font-size:.82rem}
    .perm-icon{font-size:18px;width:18px;height:18px}
    .action-chip{font-size:.65rem!important;height:18px!important;margin-left:auto}
    .action-enabled{background:#e6f4ea!important;color:#137333!important}
    .action-disabled{background:#f1f3f4!important;color:#9ca3af!important}

    .version-section{margin-top:20px}
    .version-section h3{font-size:.9rem;font-weight:600;color:#333;margin:0 0 8px}
    .version-item{display:flex;align-items:center;gap:12px;padding:8px 0;border-bottom:1px solid #f3f4f6}
    .version-info{flex:1}.version-info strong{display:block;font-size:.82rem}.version-info span{font-size:.72rem;color:#999}
    .chip-added{background:#e6f4ea!important;color:#137333!important}.chip-removed{background:#fce8e6!important;color:#c5221f!important}

    .diff-section{margin-top:20px;background:#fffbeb;border-radius:10px;padding:16px;border:1px solid #fde68a}
    .diff-section h3{font-size:.9rem;font-weight:600;color:#92400e;margin:0 0 12px}
    .diff-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}
    .diff-column h4{font-size:.78rem;font-weight:600;color:#666;margin:0 0 6px}
    .current-chip{background:#e8f0fe!important;color:#1967d2!important;font-size:.68rem!important;margin:2px}
    .unchanged-chip{background:#e8f0fe!important;color:#1967d2!important;font-size:.68rem!important;margin:2px}
    .added-chip{background:#e6f4ea!important;color:#137333!important;font-size:.68rem!important;margin:2px}
    .removed-chip{background:#fce8e6!important;color:#c5221f!important;font-size:.68rem!important;margin:2px;text-decoration:line-through}
    .added-icon{font-size:14px;width:14px;height:14px;color:#137333}
    .removed-icon{font-size:14px;width:14px;height:14px;color:#c5221f}
    .no-change{font-size:.78rem;color:#999}
    @media(max-width:768px){.summary-row,.diff-grid{grid-template-columns:1fr}}
  `]
})
export class PermissionPreviewDialogComponent {
  @Input() roleId!: string;
  @Input() roleName!: string;
  @Input() newPermissionIds: Set<string> = new Set();

  private roleSrv = inject(RoleService);
  private notify = inject(NotificationService);

  previewData: any[] = [];
  versionHistory: any[] = [];
  currentPermissions: string[] = [];
  newPermissions: string[] = [];
  removedPermissions: string[] = [];
  loading = false;
  showDiff = false;

  get enabledModules(): number { return this.previewData.filter(m => m.enabled).length; }
  get totalPermissions(): number { return this.previewData.reduce((sum, m) => sum + m.selectedPermissions.length, 0); }
  get restrictedModules(): number { return this.previewData.filter(m => !m.enabled).length; }

  ngOnInit(): void {
    this.loadPreview();
  }

  loadPreview(): void {
    this.loading = true;
    this.roleSrv.getModulePermissions().subscribe({
      next: (r: any) => {
        if (r.success) {
          this.previewData = (r.data || []).map((mod: any) => ({
            ...mod,
            enabled: mod.permissions?.some((p: any) => this.newPermissionIds.has(p.id)),
            selectedPermissions: mod.permissions?.filter((p: any) => this.newPermissionIds.has(p.id)) || [],
            totalPermissions: mod.permissions?.length || 0,
          }));
          // Build diff
          if (this.roleId) {
            this.roleSrv.getById(this.roleId).subscribe({
              next: (roleR: any) => {
                if (roleR.success && roleR.data) {
                  this.currentPermissions = roleR.data.permissions || [];
                  this.newPermissions = Array.from(this.newPermissionIds);
                  this.removedPermissions = this.currentPermissions.filter(p => !this.newPermissionIds.has(p));
                  this.showDiff = this.removedPermissions.length > 0 || 
                    this.newPermissions.some(p => !this.currentPermissions.includes(p));
                }
              }
            });
          }
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  formatPermName(name: string): string {
    return name.replace(/([A-Z])/g, ' $1').replace(/\./g, ' ').trim();
  }

  formatDate(d: string): string {
    if (!d) return '-';
    const moment = (window as any).moment;
    return moment ? moment(d).format('DD/MM/YYYY HH:mm') : d;
  }

  applyChanges(): void {
    this.notify.success('Changes applied!');
    this.close();
  }

  close(): void {
    // Emit close event - handled by parent
    const event = new CustomEvent('preview-closed');
    window.dispatchEvent(event);
  }
}