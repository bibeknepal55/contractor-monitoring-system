import { Component, inject, OnInit } from '@angular/core';
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
import { ContractorService } from '../../../core/services/contractor.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Contractor } from '../../../core/models/contractor.model';
import { debounceTime, Subject } from 'rxjs';

@Component({
  selector: 'app-contractor-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule, MatProgressBarModule, FormsModule,
  ],
  template: `
    <div class="list-page">
      <div class="list-header">
        <div>
          <h1>Contractors</h1>
          <p>{{ totalItems }} contractors total</p>
        </div>
        <button mat-flat-button color="primary" routerLink="new" *ngIf="auth.hasPermission('ContractorOfficeDetail.Create')">
          <mat-icon>add</mat-icon> New Contractor
        </button>
      </div>

      <div class="list-toolbar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="searchText" (ngModelChange)="onSearch($event)" placeholder="Search contractors...">
        </mat-form-field>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate"></mat-progress-bar>

      <div class="table-wrap" *ngIf="!loading && items.length > 0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)" class="full-table">
          <ng-container matColumnDef="companyName">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Company</th>
            <td mat-cell *matCellDef="let r" class="name-cell">{{r.companyName}}</td>
          </ng-container>
          <ng-container matColumnDef="contactPerson">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Contact Person</th>
            <td mat-cell *matCellDef="let r">{{r.contactPerson || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Email</th>
            <td mat-cell *matCellDef="let r">{{r.email || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="phone">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Phone</th>
            <td mat-cell *matCellDef="let r">{{r.phone || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="city">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>City</th>
            <td mat-cell *matCellDef="let r">{{r.city || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Status</th>
            <td mat-cell *matCellDef="let r"><span class="badge" [class]="'badge-' + r.status?.toLowerCase()">{{r.status}}</span></td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let r" class="actions-cell">
              <button mat-icon-button color="primary" (click)="edit(r)" *ngIf="auth.hasPermission('ContractorOfficeDetail.Update')"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button color="warn" (click)="deleteItem(r)" *ngIf="auth.hasPermission('ContractorOfficeDetail.Delete')"><mat-icon>delete</mat-icon></button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols" class="table-row"></tr>
        </table>
        <mat-paginator [length]="totalItems" [pageSize]="pageSize" [pageIndex]="page - 1" [pageSizeOptions]="[5,10,25]" (page)="onPage($event)" *ngIf="totalItems > 5"></mat-paginator>
      </div>

      <div class="empty" *ngIf="!loading && items.length === 0">
        <mat-icon>folder_open</mat-icon>
        <h3>No Contractors</h3>
        <p>Register your first contractor</p>
        <button mat-flat-button color="primary" routerLink="new" *ngIf="auth.hasPermission('ContractorOfficeDetail.Create')">Add Contractor</button>
      </div>
    </div>
  `,
  styles: [`
    .list-page { padding: 24px; max-width: 1400px; margin: 0 auto; }
    .list-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 12px; }
    .list-header h1 { margin: 0; font-size: 1.5rem; font-weight: 700; }
    .list-header p { margin: 2px 0 0; color: #666; font-size: 0.85rem; }
    .list-toolbar { margin-bottom: 16px; }
    .search-field { width: 320px; max-width: 100%; }
    .table-wrap { background: #fff; border-radius: 12px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); overflow: hidden; }
    .full-table { width: 100%; }
    th.mat-mdc-header-cell { background: #f9fafb; font-weight: 600; color: #444; font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.5px; padding: 14px 16px; }
    td.mat-mdc-cell { padding: 14px 16px; font-size: 0.9rem; color: #333; }
    .table-row:hover { background: #f5f7ff; }
    .name-cell { font-weight: 500; color: #1a1a1a; }
    .actions-cell { white-space: nowrap; }
    .badge { display: inline-block; padding: 3px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
    .badge-active { background: #e6f4ea; color: #137333; }
    .badge-inactive { background: #f1f3f4; color: #5f6368; }
    .badge-blacklisted { background: #fce8e6; color: #c5221f; }
    .badge-underreview { background: #fef7e0; color: #b06000; }
    .empty { text-align: center; padding: 60px 20px; color: #888; }
    .empty mat-icon { font-size: 56px; width: 56px; height: 56px; margin-bottom: 12px; color: #ccc; }
    .empty h3 { margin: 0 0 4px; font-size: 1.1rem; color: #555; }
    @media (max-width: 768px) { .list-page { padding: 16px; } }
  `]
})
export class ContractorListComponent implements OnInit {
  private readonly service = inject(ContractorService);
  readonly auth = inject(AuthService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);

  items: Contractor[] = [];
  totalItems = 0;
  page = 1;
  pageSize = 10;
  sortBy = 'createdAt';
  sortDir: 'asc' | 'desc' = 'desc';
  searchText = '';
  loading = false;
  private search$ = new Subject<string>();

  cols = ['companyName', 'contactPerson', 'email', 'phone', 'city', 'status', 'actions'];

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300)).subscribe(() => { this.page = 1; this.fetch(); });
    this.fetch();
  }

  fetch(): void {
    this.loading = true;
    this.service.getContractors({ page: this.page, pageSize: this.pageSize, search: this.searchText || undefined, sortBy: this.sortBy, sortOrder: this.sortDir }).subscribe({
      next: (r: any) => { if (r.success) { this.items = r.data; this.totalItems = r.totalCount; } this.loading = false; },
      error: () => { this.loading = false; this.notify.error('Failed'); }
    });
  }

  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }
  onSort(s: Sort): void { this.sortBy = s.active; this.sortDir = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPage(e: PageEvent): void { this.page = e.pageIndex + 1; this.pageSize = e.pageSize; this.fetch(); }
  edit(c: Contractor): void { this.router.navigate(['/contractors', c.id, 'edit']); }

  async deleteItem(c: Contractor): Promise<void> {
    const ok = await this.notify.confirmDelete(c.companyName);
    if (!ok) return;
    this.service.deleteContractor(c.id).subscribe({
      next: (r: any) => { if (r.success) { this.notify.success('Deleted'); this.fetch(); } },
      error: () => this.notify.error('Failed')
    });
  }
}