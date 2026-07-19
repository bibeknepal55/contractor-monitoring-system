import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { OrganizationService } from '../../../core/services/organization.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { HasPermissionDirective, HasAnyRoleDirective } from '../../../core/directives/has-permission.directive';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { debounceTime, Subject, Subscription } from 'rxjs';

@Component({
  selector: 'app-organization-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatCardModule, MatChipsModule, MatTooltipModule, FormsModule,
    MatDividerModule, MatSlideToggleModule,
    HasPermissionDirective, HasAnyRoleDirective, LoadingSpinnerComponent,
    EmptyStateComponent, ErrorStateComponent
  ],
  template: `
    <div class="list-page">
      <div class="list-header">
        <div>
          <h1>Departments & Divisions</h1>
          <p>{{ totalItems }} organizations</p>
        </div>
        <button mat-flat-button color="primary" (click)="openForm()" *appHasPermission="'Organization.Create'">
          <mat-icon>add</mat-icon> New Department
        </button>
      </div>

      <div class="list-toolbar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="searchText" (ngModelChange)="onSearch($event)" placeholder="Search departments...">
        </mat-form-field>
        <mat-form-field appearance="outline" class="type-field">
          <mat-label>Type</mat-label>
          <mat-select [(ngModel)]="filterType" (selectionChange)="fetch()">
            <mat-option value="">All</mat-option>
            <mat-option value="Department">Department</mat-option>
            <mat-option value="Division">Division</mat-option>
            <mat-option value="Region">Region</mat-option>
            <mat-option value="Unit">Unit</mat-option>
          </mat-select>
        </mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh"><mat-icon>refresh</mat-icon></button>
      </div>

      <app-loading-spinner *ngIf="loading"></app-loading-spinner>
      <app-error-state *ngIf="!loading && error" [message]="error" (retry)="fetch()"></app-error-state>

      <div class="org-grid" *ngIf="!loading && !error && items.length > 0">
        <mat-card *ngFor="let org of items" class="org-card">
          <mat-card-header>
            <mat-icon mat-card-avatar [style.color]="getTypeColor(org.type)">business</mat-icon>
            <mat-card-title>{{ org.name }}</mat-card-title>
            <mat-card-subtitle>
              <mat-chip class="type-chip" [style.background]="getTypeColor(org.type)+'20'" [style.color]="getTypeColor(org.type)">
                {{ org.type }}
              </mat-chip>
              {{ org.userCount || 0 }} members
            </mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p class="org-desc">{{ org.description || 'No description' }}</p>
            <div class="org-meta">
              <span *ngIf="org.parentId">Parent: {{ org.parentName || 'N/A' }}</span>
              <span [class.active]="org.isActive" [class.inactive]="!org.isActive">
                {{ org.isActive ? 'Active' : 'Inactive' }}
              </span>
            </div>
          </mat-card-content>
          <mat-divider></mat-divider>
          <mat-card-actions>
            <button mat-button color="primary" (click)="manageUsers(org)">
              <mat-icon>people</mat-icon> Users
            </button>
            <button mat-button color="primary" (click)="editOrg(org)" *appHasPermission="'Organization.Update'">
              <mat-icon>edit</mat-icon> Edit
            </button>
            <button mat-button color="warn" (click)="deleteOrg(org)" *appHasPermission="'Organization.Delete'">
              <mat-icon>delete</mat-icon> Delete
            </button>
          </mat-card-actions>
        </mat-card>
      </div>

      <app-empty-state
        *ngIf="!loading && !error && items.length === 0"
        icon="corporate_fare"
        title="No Departments Yet"
        description="Create departments to organize your team and control access at each level."
        actionLabel="New Department"
        (click)="openForm()">
      </app-empty-state>
    </div>

    <div class="dialog-overlay" *ngIf="showForm" (click)="closeForm($event)">
      <div class="dialog-box" (click)="$event.stopPropagation()">
        <div class="dialog-header">
          <h2>{{ editingOrg ? 'Edit Department' : 'New Department' }}</h2>
          <button mat-icon-button (click)="showForm=false"><mat-icon>close</mat-icon></button>
        </div>
        <mat-divider></mat-divider>
        <div class="dialog-body">
          <mat-form-field appearance="outline">
            <mat-label>Name *</mat-label>
            <input matInput [(ngModel)]="formData.name" maxlength="100" placeholder="e.g., Highway Division">
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Type *</mat-label>
            <mat-select [(ngModel)]="formData.type">
              <mat-option value="Department">Department</mat-option>
              <mat-option value="Division">Division</mat-option>
              <mat-option value="Region">Region</mat-option>
              <mat-option value="Unit">Unit</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Description</mat-label>
            <textarea matInput [(ngModel)]="formData.description" rows="2" maxlength="200"></textarea>
          </mat-form-field>
          <mat-form-field appearance="outline" *appHasAnyRole="['SuperAdmin']">
            <mat-label>Parent Department (optional)</mat-label>
            <mat-select [(ngModel)]="formData.parentId">
              <mat-option [value]="">None (Top Level)</mat-option>
              <mat-option *ngFor="let org of items" [value]="org.id" [disabled]="org.id === editingOrg?.id">
                {{ org.name }} ({{ org.type }})
              </mat-option>
            </mat-select>
          </mat-form-field>
          <mat-slide-toggle [(ngModel)]="formData.isActive" color="primary" class="active-toggle">
            {{ formData.isActive ? 'Active' : 'Inactive' }}
          </mat-slide-toggle>
        </div>
        <div class="dialog-actions">
          <button mat-stroked-button (click)="showForm=false">Cancel</button>
          <button mat-flat-button color="primary" (click)="saveOrg()" [disabled]="!formData.name || saving">
            {{ saving ? 'Saving...' : (editingOrg ? 'Update' : 'Create') }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .list-page{padding:24px;max-width:1400px;margin:0 auto}
    .list-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;flex-wrap:wrap;gap:12px}
    .list-header h1{font-size:1.5rem;font-weight:700;margin:0}.list-header p{color:#666;font-size:.85rem}
    .list-toolbar{display:flex;align-items:center;gap:8px;margin-bottom:16px}
    .search-field{width:320px}.type-field{width:160px}
    .org-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(350px,1fr));gap:16px}
    .org-card{border-radius:12px;border:1px solid #e8eaed;box-shadow:0 1px 3px rgba(0,0,0,.04)}
    .org-card mat-card-header{padding:20px 20px 0}
    .type-chip{font-size:.68rem!important;height:20px!important;font-weight:600!important;margin-right:8px}
    .org-desc{color:#666;font-size:.85rem;margin:8px 0}.org-meta{display:flex;gap:16px;font-size:.75rem;color:#999}
    .org-meta .active{color:#137333}.org-meta .inactive{color:#c5221f}
    .dialog-overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1000;display:flex;align-items:center;justify-content:center}
    .dialog-box{background:#fff;border-radius:16px;width:90%;max-width:550px;max-height:85vh;display:flex;flex-direction:column;box-shadow:0 20px 60px rgba(0,0,0,.3)}
    .dialog-header{display:flex;justify-content:space-between;align-items:center;padding:20px 24px 0}
    .dialog-header h2{font-size:1.2rem;font-weight:600;margin:0}
    .dialog-body{padding:16px 24px;overflow-y:auto;flex:1}
    .dialog-actions{display:flex;justify-content:flex-end;gap:12px;padding:16px 24px;border-top:1px solid #eee}
    .active-toggle{margin-top:8px}
    mat-form-field{width:100%}
    @media(max-width:768px){.list-page{padding:16px}.org-grid{grid-template-columns:1fr}.search-field,.type-field{width:100%}}
  `]
})
export class OrganizationListComponent implements OnInit, OnDestroy {
  private srv = inject(OrganizationService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);

  items: any[] = [];
  totalItems = 0;
  loading = false;
  error: string | null = null;
  searchText = '';
  filterType = '';
  private search$ = new Subject<string>();
  private storeSub!: Subscription;

  showForm = false;
  editingOrg: any = null;
  saving = false;
  formData: any = { name: '', type: 'Department', description: '', parentId: '', isActive: true };

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300)).subscribe(() => this.fetch());
    this.storeSub = this.srv.orgs$.subscribe(s => {
      this.items = s.data; this.totalItems = s.total;
      this.loading = s.loading; this.error = s.error;
    });
    this.fetch();
  }

  ngOnDestroy(): void { if (this.storeSub) this.storeSub.unsubscribe(); this.srv.clearStore(); }

  fetch(): void { this.srv.loadOrgs({ page: 1, pageSize: 100, search: this.searchText || undefined }); }
  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }

  openForm(org?: any): void {
    this.editingOrg = org || null;
    this.formData = org ? { ...org } : { name: '', type: 'Department', description: '', parentId: '', isActive: true };
    this.showForm = true;
  }

  editOrg(org: any): void { this.openForm(org); }
  closeForm(event: MouseEvent): void { if ((event.target as HTMLElement).classList.contains('dialog-overlay')) this.showForm = false; }

  saveOrg(): void {
    if (!this.formData.name.trim()) return;
    this.saving = true;
    const req = this.editingOrg
      ? this.srv.updateAndRefresh(this.editingOrg.id, this.formData)
      : this.srv.createAndRefresh(this.formData);
    req.subscribe({
      next: (r: any) => {
        this.saving = false;
        if (r.success) { this.notify.success(this.editingOrg ? 'Updated!' : 'Created!'); this.showForm = false; }
        else this.notify.error(r.message || 'Failed');
      },
      error: (e: any) => { this.saving = false; this.notify.error(e?.error?.message || 'Failed'); }
    });
  }

  async deleteOrg(org: any): Promise<void> {
    const ok = await this.notify.confirmAction('Delete Department', `Delete "${org.name}"? Users in this department will need reassignment.`);
    if (!ok) return;
    this.srv.deleteAndRefresh(org.id).subscribe({
      next: (r: any) => { if (r.success) this.notify.success('Deleted!'); else this.notify.error(r.message || 'Failed'); },
      error: (e: any) => this.notify.error('Failed to delete')
    });
  }

  manageUsers(org: any): void {
    window.location.href = `/users?orgId=${org.id}`;
  }

  getTypeColor(type: string): string {
    const c: any = { Department: '#1976d2', Division: '#388e3c', Region: '#f57c00', Unit: '#9c27b0' };
    return c[type] || '#757575';
  }
}