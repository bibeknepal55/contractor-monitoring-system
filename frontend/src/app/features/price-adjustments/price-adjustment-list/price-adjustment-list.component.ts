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
import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';
import { PriceAdjustmentService } from '../../../core/services/price-adjustment.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-price-adjustment-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule,
    MatProgressBarModule, MatTooltipModule, MatChipsModule, FormsModule,
  ],
  template: `
    <div class="list-page">
      <div class="list-header">
        <div>
          <h1>Price Adjustments</h1>
          <p>{{ total }} adjustments total</p>
        </div>
        <button mat-flat-button color="primary" routerLink="new"
          *ngIf="auth.hasPermission('PriceAdjustment.Create')">
          <mat-icon>add</mat-icon> New Adjustment
        </button>
      </div>

      <div class="list-toolbar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [(ngModel)]="searchText" (ngModelChange)="onSearch($event)" placeholder="Search adjustments...">
        </mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh" style="margin-left:8px">
          <mat-icon>refresh</mat-icon>
        </button>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate"></mat-progress-bar>

      <div class="table-wrap" *ngIf="!loading && items.length > 0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)" class="full-table">
          
          <ng-container matColumnDef="projectName">
            <th mat-header-cell *matHeaderCellDef>Project</th>
            <td mat-cell *matCellDef="let r" class="name-cell">{{ r.projectName || (r.projectId | slice:0:8) || '-' }}</td>
          </ng-container>

          <ng-container matColumnDef="adjustmentType">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Type</th>
            <td mat-cell *matCellDef="let r">
              <mat-chip style="font-size:0.7rem;font-weight:600;" 
                [style.background-color]="getTypeColor(r.adjustmentType)+'20'" 
                [style.color]="getTypeColor(r.adjustmentType)">
                {{ r.adjustmentType || '-' }}
              </mat-chip>
            </td>
          </ng-container>

          <ng-container matColumnDef="amount">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Amount</th>
            <td mat-cell *matCellDef="let r">
              <span style="font-weight:600;">
                ₹{{ (r.newAmount || r.amount || 0) | number:'1.0-0' }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="percentageChange">
            <th mat-header-cell *matHeaderCellDef>% Change</th>
            <td mat-cell *matCellDef="let r">
              <span class="badge" [class.badge-positive]="(r.percentageChange||0)>=0" [class.badge-negative]="(r.percentageChange||0)<0">
                {{ (r.percentageChange||0) >= 0 ? '+' : '' }}{{ r.percentageChange || 0 }}%
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="isApproved">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Status</th>
            <td mat-cell *matCellDef="let r">
              <span class="badge" [class.badge-approved]="r.isApproved" [class.badge-pending]="!r.isApproved">
                {{ r.isApproved ? '✓ Approved' : '⏳ Pending' }}
              </span>
            </td>
          </ng-container>

          <!-- Dynamic Date Column: Shows Adjustment Date when Pending, both when Approved -->
          <ng-container matColumnDef="dates">
            <th mat-header-cell *matHeaderCellDef>Dates</th>
            <td mat-cell *matCellDef="let r">
              <!-- Pending: Show only Adjustment Date -->
              <div *ngIf="!r.isApproved" style="display:flex;align-items:center;gap:4px;">
                <mat-icon style="font-size:14px;width:14px;height:14px;color:#f57c00;">event</mat-icon>
                <span style="font-size:0.8rem;color:#666;">
                  Adj: {{ r.adjustmentDate ? formatDate(r.adjustmentDate) : '-' }}
                </span>
              </div>
              <!-- Approved: Show both Adjustment Date + Approval Date -->
              <div *ngIf="r.isApproved" style="display:flex;flex-direction:column;gap:2px;">
                <div style="display:flex;align-items:center;gap:4px;">
                  <mat-icon style="font-size:14px;width:14px;height:14px;color:#f57c00;">event</mat-icon>
                  <span style="font-size:0.8rem;color:#666;">
                    Adj: {{ r.adjustmentDate ? formatDate(r.adjustmentDate) : '-' }}
                  </span>
                </div>
                <div style="display:flex;align-items:center;gap:4px;">
                  <mat-icon style="font-size:14px;width:14px;height:14px;color:#137333;">check_circle</mat-icon>
                  <span style="font-size:0.8rem;color:#137333;font-weight:500;">
                    Appr: {{ r.effectiveDate ? formatDate(r.effectiveDate) : 'Approved' }}
                  </span>
                </div>
              </div>
            </td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let r" class="actions-cell">
              <button mat-icon-button color="primary" (click)="edit(r)" matTooltip="Edit"
                *ngIf="auth.hasPermission('PriceAdjustment.Update')">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteItem(r)" matTooltip="Delete"
                *ngIf="auth.hasPermission('PriceAdjustment.Delete')">
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
        <mat-icon>trending_flat</mat-icon>
        <h3>No Price Adjustments</h3>
        <p>Record your first price adjustment</p>
        <button mat-flat-button color="primary" routerLink="new"
          *ngIf="auth.hasPermission('PriceAdjustment.Create')">
          Add Adjustment
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
    .name-cell { font-weight: 500; color: #1a1a1a; }
    .actions-cell { white-space: nowrap; }
    .badge { display: inline-block; padding: 3px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
    .badge-approved { background: #e6f4ea; color: #137333; }
    .badge-pending { background: #fef7e0; color: #b06000; }
    .badge-positive { background: #e6f4ea; color: #137333; }
    .badge-negative { background: #fce8e6; color: #c5221f; }
    .empty { text-align: center; padding: 60px 20px; color: #888; }
    .empty mat-icon { font-size: 56px; width: 56px; height: 56px; margin-bottom: 12px; color: #ccc; }
    .empty h3 { margin: 0 0 4px; font-size: 1.1rem; color: #555; }
    @media (max-width: 768px) { .list-page { padding: 16px; } .search-field { width: 100%; } }
  `]
})
export class PriceAdjustmentListComponent implements OnInit {
  private srv = inject(PriceAdjustmentService);
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

  cols = ['projectName', 'adjustmentType', 'amount', 'percentageChange', 'isApproved', 'dates', 'actions'];

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
        if (r.success) { 
          this.items = r.data; 
          this.total = r.totalCount; 
          this.cdr.detectChanges(); 
        }
        this.loading = false;
      },
      error: () => { this.loading = false; this.notify.error('Failed to load'); }
    });
  }

  getTypeColor(type: string): string {
    const colors: Record<string, string> = {
      'Escalation': '#f57c00', 'Material': '#1976d2', 'Labor': '#388e3c',
      'Scope Change': '#9c27b0', 'Regulatory': '#d32f2f', 'Other': '#757575'
    };
    return colors[type] || '#757575';
  }

  onSearch(v: string): void { this.searchText = v; this.search$.next(v); }
  onSort(s: Sort): void { this.sortBy = s.active; this.sortDir = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPage(e: PageEvent): void { this.page = e.pageIndex + 1; this.pageSize = e.pageSize; this.fetch(); }
  formatDate(d: string): string { 
    if (!d) return '-';
    return moment(d).format('DD/MM/YYYY'); 
  }
  edit(r: any): void { this.router.navigate(['/price-adjustments', r.id, 'edit']); }

  async deleteItem(r: any): Promise<void> {
    const ok = await this.notify.confirmDelete('this adjustment');
    if (!ok) return;
    this.srv.delete(r.id).subscribe({
      next: (x) => { 
        if (x.success) { 
          this.notify.success('Deleted'); 
          this.fetch(); 
        } else {
          this.notify.error(x.message || 'Failed'); 
        }
      },
      error: () => this.notify.error('Delete failed')
    });
  }
}