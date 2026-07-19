import { Component, inject, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
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
import { DateFormatService } from '../../../core/services/date-format.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { debounceTime, Subject, Subscription } from 'rxjs';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule,
    MatProgressBarModule, FormsModule, LoadingSpinnerComponent, EmptyStateComponent,
    ErrorStateComponent
  ],
  template: `
    <div class="list-page">
      <div class="list-header">
        <div>
          <h1>Projects</h1>
          <p>{{ totalItems }} projects total</p>
        </div>
        <button mat-flat-button color="primary" routerLink="new" *appHasPermission="'Project.Create'">
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

      <!-- Loading State -->
      <app-loading-spinner *ngIf="loading"></app-loading-spinner>

      <!-- Error State -->
      <app-error-state 
        *ngIf="!loading && error" 
        [message]="error"
        (retry)="fetch()">
      </app-error-state>

      <!-- Data Table -->
      <div class="table-wrap" *ngIf="!loading && !error && items.length > 0">
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
            <td mat-cell *matCellDef="let r">{{r.startDate ? dateFmt.formatDate(r.startDate) : '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let r" class="actions-cell">
              <button mat-icon-button color="primary" (click)="edit(r)" *appHasPermission="'Project.Update'" matTooltip="Edit">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteItem(r)" *appHasPermission="'Project.Delete'" matTooltip="Delete">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols" class="table-row"></tr>
        </table>
        <mat-paginator [length]="totalItems" [pageSize]="pageSize" [pageIndex]="page - 1" 
          [pageSizeOptions]="[5,10,25]" (page)="onPage($event)" *ngIf="totalItems > 5" showFirstLastButtons>
        </mat-paginator>
      </div>

      <!-- Empty State -->
      <app-empty-state 
        *ngIf="!loading && !error && items.length === 0"
        icon="folder_open"
        title="No Projects Found"
        description="Create your first project to get started with contractor monitoring."
        actionLabel="New Project"
        actionRoute="new"
        actionIcon="add">
      </app-empty-state>
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
    @media (max-width: 768px) { .list-page { padding: 16px; } .search-field { width: 100%; } }
  `]
})
export class ProjectListComponent implements OnInit, OnDestroy {
  private service = inject(ProjectService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  readonly dateFmt = inject(DateFormatService);

  items: any[] = [];
  totalItems = 0;
  page = 1;
  pageSize = 10;
  sortBy = 'createdAt';
  sortDir: 'asc' | 'desc' = 'desc';
  searchText = '';
  loading = false;
  error: string | null = null;
  private search$ = new Subject<string>();
  private storeSub!: Subscription;

  cols = ['projectCode', 'projectName', 'status', 'priority', 'budget', 'projectManager', 'startDate', 'actions'];

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300)).subscribe(() => { this.page = 1; this.fetch(); });

    // Subscribe to reactive store for automatic updates
    this.storeSub = this.service.projects$.subscribe(state => {
      this.items = state.data;
      this.totalItems = state.total;
      this.loading = state.loading;
      this.error = state.error;
      this.cdr.detectChanges();
    });

    this.fetch();
  }

  ngOnDestroy(): void {
    if (this.storeSub) this.storeSub.unsubscribe();
    this.service.clearStore();
  }

  fetch(): void {
    this.service.loadProjects({
      page: this.page,
      pageSize: this.pageSize,
      search: this.searchText || undefined,
      sortBy: this.sortBy,
      sortOrder: this.sortDir
    });
  }

  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }
  onSort(s: Sort): void { this.sortBy = s.active; this.sortDir = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPage(e: PageEvent): void { this.page = e.pageIndex + 1; this.pageSize = e.pageSize; this.fetch(); }
  edit(p: any): void { this.router.navigate(['/projects', p.id, 'edit']); }

  async deleteItem(p: any): Promise<void> {
    const ok = await this.notify.confirmDelete(p.projectName || 'this project');
    if (!ok) return;
    this.service.deleteAndRefresh(p.id).subscribe({
      next: (r) => {
        if (r.success) this.notify.success('Project deleted');
        else this.notify.error(r.message || 'Failed');
      },
      error: () => this.notify.error('Failed to delete project')
    });
  }
}