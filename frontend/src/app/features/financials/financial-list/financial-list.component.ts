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
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { FinancialService } from '../../../core/services/financial.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-financial-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule,
    MatProgressBarModule, MatTooltipModule, FormsModule,
  ],
  template: `
    <div class="list-page">
      <div class="list-header">
        <div>
          <h1>Contract Financials</h1>
          <p>{{ total }} records total</p>
        </div>
        <button mat-flat-button color="primary" routerLink="new"
          *ngIf="auth.hasPermission('ContractFinancialDetail.Create')">
          <mat-icon>add</mat-icon> Add Financial
        </button>
      </div>

      <div class="list-toolbar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="searchText" (ngModelChange)="onSearch($event)" placeholder="Search financials...">
        </mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh" style="margin-left:8px">
          <mat-icon>refresh</mat-icon>
        </button>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate"></mat-progress-bar>

      <div class="table-wrap" *ngIf="!loading && items.length > 0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)" class="full-table">
          <ng-container matColumnDef="projectId">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Project</th>
            <td mat-cell *matCellDef="let r">{{ r.projectName || r.projectId?.substring(0,8) || '-' }}</td>
          </ng-container>
          <ng-container matColumnDef="contractAmount">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Contract Amount</th>
            <td mat-cell *matCellDef="let r">₹{{ (r.contractAmount || 0) | number:'1.0-0' }}</td>
          </ng-container>
          <ng-container matColumnDef="advancePayment">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Advance</th>
            <td mat-cell *matCellDef="let r">₹{{ (r.advancePayment || 0) | number:'1.0-0' }}</td>
          </ng-container>
          <ng-container matColumnDef="currency">
            <th mat-header-cell *matHeaderCellDef>Currency</th>
            <td mat-cell *matCellDef="let r">{{ r.currency || 'INR' }}</td>
          </ng-container>
          <ng-container matColumnDef="bankName">
            <th mat-header-cell *matHeaderCellDef>Bank</th>
            <td mat-cell *matCellDef="let r">{{ r.bankName || '-' }}</td>
          </ng-container>
          <ng-container matColumnDef="contractSigningDate">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Signing Date</th>
            <td mat-cell *matCellDef="let r">{{ r.contractSigningDate ? formatDate(r.contractSigningDate) : '-' }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let r" class="actions-cell">
              <button mat-icon-button color="primary" (click)="edit(r)" matTooltip="Edit"
                *ngIf="auth.hasPermission('ContractFinancialDetail.Update')">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteItem(r)" matTooltip="Delete"
                *ngIf="auth.hasPermission('ContractFinancialDetail.Delete')">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols" class="table-row"></tr>
        </table>
        <mat-paginator
          [length]="total" [pageSize]="pageSize" [pageIndex]="page - 1"
          [pageSizeOptions]="[5,10,25]" (page)="onPage($event)"
          *ngIf="total > 5" showFirstLastButtons>
        </mat-paginator>
      </div>

      <div class="empty" *ngIf="!loading && items.length === 0">
        <mat-icon>folder_open</mat-icon>
        <h3>No Financial Records</h3>
        <p>Add your first contract financial record</p>
        <button mat-flat-button color="primary" routerLink="new"
          *ngIf="auth.hasPermission('ContractFinancialDetail.Create')">
          Add Record
        </button>
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
    .actions-cell { white-space: nowrap; }
    .empty { text-align: center; padding: 60px 20px; color: #888; }
    .empty mat-icon { font-size: 56px; width: 56px; height: 56px; margin-bottom: 12px; color: #ccc; }
    .empty h3 { margin: 0 0 4px; font-size: 1.1rem; color: #555; }
    @media (max-width: 768px) { .list-page { padding: 16px; } .search-field { width: 100%; } }
  `]
})
export class FinancialListComponent implements OnInit {
  private srv = inject(FinancialService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  items: any[] = [];
  total = 0;
  page = 1;
  pageSize = 10;
  sortBy = 'createdAt';
  sortDir: 'asc' | 'desc' = 'desc';
  searchText = '';
  loading = false;
  private search$ = new Subject<string>();

  cols = ['projectId', 'contractAmount', 'advancePayment', 'currency', 'bankName', 'contractSigningDate', 'actions'];

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300)).subscribe(() => { this.page = 1; this.fetch(); });
    this.fetch();
  }

  fetch(): void {
    this.loading = true;
    this.srv.getAll({
      page: this.page, pageSize: this.pageSize,
      search: this.searchText || undefined,
      sortBy: this.sortBy, sortOrder: this.sortDir
    }).subscribe({
      next: (r) => {
        if (r.success) { this.items = r.data; this.total = r.totalCount; this.cdr.detectChanges(); }
        this.loading = false;
      },
      error: () => { this.loading = false; this.notify.error('Failed to load financials'); }
    });
  }

  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }
  onSort(s: Sort): void { this.sortBy = s.active; this.sortDir = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPage(e: PageEvent): void { this.page = e.pageIndex + 1; this.pageSize = e.pageSize; this.fetch(); }
  formatDate(d: string): string { return moment(d).format('DD/MM/YYYY'); }
  edit(r: any): void { this.router.navigate(['/financials', r.id, 'edit']); }

  async deleteItem(r: any): Promise<void> {
    const ok = await this.notify.confirmDelete('this financial record');
    if (!ok) return;
    this.srv.delete(r.id).subscribe({
      next: (x) => { if (x.success) { this.notify.success('Deleted'); this.fetch(); } else this.notify.error(x.message || 'Failed'); },
      error: () => this.notify.error('Failed to delete')
    });
  }
}