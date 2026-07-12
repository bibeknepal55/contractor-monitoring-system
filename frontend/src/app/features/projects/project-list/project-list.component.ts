import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Project } from '../../../core/models/project.model';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule, MatProgressBarModule, FormsModule,
  ],
  template: `
    <div class="list-page">
      <div class="list-header">
        <div>
          <h1>Projects</h1>
          <p>{{ totalItems }} projects total</p>
        </div>
        <button mat-flat-button color="primary" routerLink="new" *ngIf="auth.hasPermission('Project.Create')">
          <mat-icon>add</mat-icon> New Project
        </button>
      </div>

      <div class="list-toolbar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="searchText" (ngModelChange)="onSearch($event)" placeholder="Search projects...">
        </mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh" style="margin-left:8px">
          <mat-icon>refresh</mat-icon>
        </button>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate"></mat-progress-bar>

      <div class="table-wrap" *ngIf="!loading && items.length > 0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)" class="full-table">
          <ng-container matColumnDef="projectCode">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Code</th>
            <td mat-cell *matCellDef="let r">{{r.projectCode || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="projectName">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Project Name</th>
            <td mat-cell *matCellDef="let r" class="name-cell">{{r.projectName || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Status</th>
            <td mat-cell *matCellDef="let r"><span class="badge" [class]="'badge-' + (r.status || '').toLowerCase()">{{r.status || '-'}}</span></td>
          </ng-container>
          <ng-container matColumnDef="priority">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Priority</th>
            <td mat-cell *matCellDef="let r">{{r.priority || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="budget">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Budget</th>
            <td mat-cell *matCellDef="let r">₹{{(r.budget || 0) | number:'1.0-0'}}</td>
          </ng-container>
          <ng-container matColumnDef="projectManager">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Manager</th>
            <td mat-cell *matCellDef="let r">{{r.projectManager || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="startDate">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Start Date</th>
            <td mat-cell *matCellDef="let r">{{r.startDate ? formatDate(r.startDate) : '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let r" class="actions-cell">
              <button mat-icon-button color="primary" (click)="edit(r)" *ngIf="auth.hasPermission('Project.Update')" matTooltip="Edit"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button color="warn" (click)="deleteItem(r)" *ngIf="auth.hasPermission('Project.Delete')" matTooltip="Delete"><mat-icon>delete</mat-icon></button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols" class="table-row"></tr>
        </table>
        <mat-paginator [length]="totalItems" [pageSize]="pageSize" [pageIndex]="page - 1" [pageSizeOptions]="[5,10,25]" (page)="onPage($event)" *ngIf="totalItems > 5" showFirstLastButtons></mat-paginator>
      </div>

      <div class="empty" *ngIf="!loading && items.length === 0">
        <mat-icon>folder_open</mat-icon>
        <h3>No Projects</h3>
        <p>Create your first project to get started</p>
        <button mat-flat-button color="primary" routerLink="new" *ngIf="auth.hasPermission('Project.Create')">Create Project</button>
      </div>
    </div>
  `,
  styles: [`
    .list-page { padding: 24px; max-width: 1400px; margin: 0 auto; }
    .list-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 12px; }
    .list-header h1 { margin: 0; font-size: 1.5rem; font-weight: 700; }
    .list-header p { margin: 2px 0 0; color: #666; font-size: 0.85rem; }
    .list-toolbar { display: flex; align-items: center; margin-bottom: 16px; }
    .search-field { width: 320px; max-width: 100%; }
    .table-wrap { background: #fff; border-radius: 12px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); overflow: hidden; }
    .full-table { width: 100%; }
    th.mat-mdc-header-cell { background: #f9fafb; font-weight: 600; color: #444; font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.5px; padding: 14px 16px; white-space: nowrap; }
    td.mat-mdc-cell { padding: 14px 16px; font-size: 0.88rem; color: #333; white-space: nowrap; }
    .table-row:hover { background: #f5f7ff; }
    .name-cell { font-weight: 500; color: #1a1a1a; }
    .actions-cell { white-space: nowrap; }
    .badge { display: inline-block; padding: 3px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
    .badge-active, .badge-completed { background: #e6f4ea; color: #137333; }
    .badge-planned { background: #e8f0fe; color: #1967d2; }
    .badge-onhold { background: #fef7e0; color: #b06000; }
    .badge-delayed { background: #fce8e6; color: #c5221f; }
    .badge-cancelled { background: #f1f3f4; color: #5f6368; }
    .empty { text-align: center; padding: 60px 20px; color: #888; }
    .empty mat-icon { font-size: 56px; width: 56px; height: 56px; margin-bottom: 12px; color: #ccc; }
    .empty h3 { margin: 0 0 4px; font-size: 1.1rem; color: #555; }
    @media (max-width: 768px) { .list-page { padding: 16px; } .search-field { width: 100%; } }
  `]
})
export class ProjectListComponent implements OnInit {
  private service = inject(ProjectService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  items: Project[] = [];
  totalItems = 0;
  page = 1;
  pageSize = 10;
  sortBy = 'createdAt';
  sortDir: 'asc' | 'desc' = 'desc';
  searchText = '';
  loading = false;
  private search$ = new Subject<string>();

  cols = ['projectCode', 'projectName', 'status', 'priority', 'budget', 'projectManager', 'startDate', 'actions'];

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300)).subscribe(() => { this.page = 1; this.fetch(); });
    this.fetch();
  }

  fetch(): void {
    this.loading = true;
    this.service.getProjects({
      page: this.page, pageSize: this.pageSize,
      search: this.searchText || undefined,
      sortBy: this.sortBy, sortOrder: this.sortDir
    }).subscribe({
      next: (r) => {
        if (r.success) {
          this.items = r.data;
          this.totalItems = r.totalCount;
          console.log('Projects loaded:', this.items.length, 'Total:', this.totalItems);
          this.cdr.detectChanges();
        }
        this.loading = false;
      },
      error: () => { this.loading = false; this.notify.error('Failed to load projects'); }
    });
  }

  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }
  onSort(s: Sort): void { this.sortBy = s.active; this.sortDir = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPage(e: PageEvent): void { this.page = e.pageIndex + 1; this.pageSize = e.pageSize; this.fetch(); }
  formatDate(d: string): string { return moment(d).format('DD/MM/YYYY'); }
  edit(p: Project): void { this.router.navigate(['/projects', p.id, 'edit']); }

  async deleteItem(p: Project): Promise<void> {
    const ok = await this.notify.confirmDelete(p.projectName || 'this project');
    if (!ok) return;
    this.service.deleteProject(p.id).subscribe({
      next: (r) => { if (r.success) { this.notify.success('Project deleted'); this.fetch(); } else this.notify.error(r.message || 'Failed'); },
      error: () => this.notify.error('Failed to delete project')
    });
  }
}