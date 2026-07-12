import { Component, Input, Output, EventEmitter, ViewChild, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  type?: 'text' | 'date' | 'currency' | 'badge' | 'chip' | 'progress' | 'actions';
  width?: string;
  minWidth?: string;
  badgeColors?: Record<string, string>;
  chipColors?: Record<string, string>;
}

export interface TableAction {
  label: string;
  icon: string;
  color?: 'primary' | 'accent' | 'warn';
  permission?: string;
  visible?: (row: any) => boolean;
  disabled?: (row: any) => boolean;
  action: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatSortModule,
    MatFormFieldModule, MatInputModule, MatIconModule, MatButtonModule, MatMenuModule,
    MatTooltipModule, MatCheckboxModule, MatChipsModule, MatProgressBarModule, MatDividerModule,
  ],
  template: `
    <div class="table-container">
      <div class="table-toolbar">
        <div class="table-toolbar-left"><ng-content select="[toolbar-left]"></ng-content></div>
        <div class="table-toolbar-right">
          <mat-form-field appearance="outline" class="search-field" *ngIf="showSearch">
            <mat-icon matPrefix>search</mat-icon>
            <input matInput [placeholder]="searchPlaceholder" [(ngModel)]="searchValue" (ngModelChange)="onSearchChange($event)" [disabled]="isLoading">
            <button mat-icon-button matSuffix *ngIf="searchValue" (click)="clearSearch()" type="button"><mat-icon>close</mat-icon></button>
          </mat-form-field>
          <ng-content select="[toolbar-right]"></ng-content>
        </div>
      </div>
      <mat-progress-bar *ngIf="isLoading" mode="indeterminate" color="primary"></mat-progress-bar>
      <div class="table-scroll">
        <table mat-table [dataSource]="dataSource" matSort [matSortActive]="sortBy" [matSortDirection]="sortOrder" (matSortChange)="onSortChange($event)">
          <ng-container matColumnDef="select" *ngIf="showSelect">
            <th mat-header-cell *matHeaderCellDef>
              <mat-checkbox (change)="toggleAllRows()" [checked]="selection.size > 0 && isAllSelected()" [indeterminate]="selection.size > 0 && !isAllSelected()"></mat-checkbox>
            </th>
            <td mat-cell *matCellDef="let row">
              <mat-checkbox (click)="$event.stopPropagation()" (change)="toggleRow(row)" [checked]="selection.has(row)"></mat-checkbox>
            </td>
          </ng-container>
          <ng-container *ngFor="let column of columns" [matColumnDef]="column.key">
            <th mat-header-cell *matHeaderCellDef [mat-sort-header]="column.sortable !== false ? column.key : ''" [style.min-width]="column.minWidth" [style.width]="column.width">{{ column.label }}</th>
            <td mat-cell *matCellDef="let row" [style.min-width]="column.minWidth" [style.width]="column.width">
              <ng-container [ngSwitch]="column.type">
                <span *ngSwitchCase="'date'">{{ getCellValue(row, column.key) | date:'dd/MM/yyyy' }}</span>
                <span *ngSwitchCase="'currency'">₹{{ getCellValue(row, column.key) | number:'1.0-0' }}</span>
                <span *ngSwitchCase="'badge'" class="status-badge" [style.background-color]="getBadgeColor(row, column)" [style.color]="getBadgeTextColor(row, column)">{{ getCellValue(row, column.key) }}</span>
                <mat-chip *ngSwitchCase="'chip'" [style.background-color]="getChipColor(row, column) + '20'" [style.color]="getChipColor(row, column)" style="font-size:0.75rem;font-weight:500;">{{ getCellValue(row, column.key) }}</mat-chip>
                <div *ngSwitchCase="'progress'" class="table-progress"><mat-progress-bar mode="determinate" [value]="getCellValue(row, column.key)"></mat-progress-bar><span>{{ getCellValue(row, column.key) }}%</span></div>
                <div *ngSwitchCase="'actions'" class="table-actions">
                  <button *ngFor="let action of actions" mat-icon-button [matTooltip]="action.label" [color]="action.color || 'primary'" [disabled]="action.disabled ? action.disabled(row) : false" (click)="onAction(action.action, row)" type="button"><mat-icon>{{ action.icon }}</mat-icon></button>
                </div>
                <span *ngSwitchDefault>{{ getCellValue(row, column.key) }}</span>
              </ng-container>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns; sticky: true"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns" (click)="onRowClick(row)" [class.clickable]="rowClick.observed"></tr>
        </table>
      </div>
      <div class="empty-table" *ngIf="!isLoading && dataSource.data.length === 0">
        <mat-icon>{{ emptyIcon }}</mat-icon>
        <h3>{{ emptyTitle }}</h3>
        <p>{{ emptyMessage }}</p>
        <ng-content select="[empty-action]"></ng-content>
      </div>
      <mat-paginator
        *ngIf="totalCount > pageSizeOptions[0]"
        [length]="totalCount"
        [pageSize]="pageSize"
        [pageIndex]="currentPage - 1"
        [pageSizeOptions]="pageSizeOptions"
        (page)="onPageChange($event)"
        showFirstLastButtons
        [disabled]="isLoading">
      </mat-paginator>
    </div>
  `,
  styles: [`
    .table-container { background: white; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); overflow: hidden; }
    .table-toolbar { display: flex; align-items: center; justify-content: space-between; padding: 12px 16px; gap: 12px; flex-wrap: wrap; }
    .table-toolbar-left { display: flex; align-items: center; gap: 8px; }
    .table-toolbar-right { display: flex; align-items: center; gap: 8px; }
    .search-field { width: 280px; max-width: 100%; }
    .search-field ::ng-deep .mat-mdc-form-field-subscript-wrapper { display: none; }
    .search-field ::ng-deep .mat-mdc-form-field-infix { padding-top: 10px; padding-bottom: 10px; min-height: 44px; }
    .table-scroll { overflow-x: auto; }
    table { width: 100%; white-space: nowrap; }
    th.mat-mdc-header-cell { background: #fafafa; font-weight: 600; font-size: 0.8rem; color: #616161; text-transform: uppercase; letter-spacing: 0.5px; padding: 12px 16px; border-bottom: 2px solid #e0e0e0; position: sticky; top: 0; z-index: 10; }
    td.mat-mdc-cell { padding: 12px 16px; font-size: 0.85rem; color: #424242; border-bottom: 1px solid #f0f0f0; }
    tr.mat-mdc-row:hover { background: #fafafa; }
    tr.clickable { cursor: pointer; }
    .table-progress { display: flex; align-items: center; gap: 8px; min-width: 120px; }
    .table-progress mat-progress-bar { flex: 1; max-width: 80px; }
    .table-progress span { font-size: 0.8rem; font-weight: 600; color: #616161; min-width: 32px; }
    .table-actions { display: flex; gap: 4px; }
    .empty-table { text-align: center; padding: 48px 24px; }
    .empty-table mat-icon { font-size: 56px; width: 56px; height: 56px; color: #bdbdbd; margin-bottom: 12px; }
    .empty-table h3 { font-size: 1.1rem; color: #616161; margin: 0 0 4px; }
    .empty-table p { color: #9e9e9e; margin: 0 0 16px; font-size: 0.9rem; }
    @media (max-width: 768px) { .search-field { width: 100%; } .table-toolbar { flex-direction: column; align-items: stretch; } .table-toolbar-right { flex-direction: column; } }
  `]
})
export class DataTableComponent implements OnInit, OnChanges {
  @Input() columns: TableColumn[] = [];
  @Input() actions: TableAction[] = [];
  @Input() data: any[] = [];
  @Input() totalCount: number = 0;
  @Input() pageSize: number = 10;
  @Input() currentPage: number = 1;
  @Input() sortBy: string = '';
  @Input() sortOrder: 'asc' | 'desc' = 'asc';
  @Input() isLoading: boolean = false;
  @Input() showSearch: boolean = true;
  @Input() searchPlaceholder: string = 'Search...';
  @Input() showSelect: boolean = false;
  @Input() emptyIcon: string = 'inbox';
  @Input() emptyTitle: string = 'No data found';
  @Input() emptyMessage: string = 'There are no records to display.';
  @Input() pageSizeOptions: number[] = [5, 10, 25, 50];

  @Output() searchChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<{ sortBy: string; sortOrder: 'asc' | 'desc' }>();
  @Output() pageChange = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() actionClick = new EventEmitter<{ action: string; row: any }>();
  @Output() rowClick = new EventEmitter<any>();
  @Output() selectionChange = new EventEmitter<any[]>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<any>([]);
  selection = new Set<any>();
  searchValue: string = '';
  private searchSubject = new Subject<string>();

  get displayedColumns(): string[] {
    const cols = [...this.columns.map(c => c.key)];
    if (this.showSelect) cols.unshift('select');
    return cols;
  }

  ngOnInit(): void {
    this.searchSubject.pipe(debounceTime(300), distinctUntilChanged()).subscribe(v => this.searchChange.emit(v));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data']) { this.dataSource.data = this.data || []; this.selection.clear(); }
  }

  onSearchChange(value: string): void { this.searchSubject.next(value); }
  clearSearch(): void { this.searchValue = ''; this.searchSubject.next(''); }
  onSortChange(sort: Sort): void { this.sortChange.emit({ sortBy: sort.active, sortOrder: sort.direction === 'desc' ? 'desc' : 'asc' }); }
  onPageChange(event: PageEvent): void { this.pageChange.emit({ page: event.pageIndex + 1, pageSize: event.pageSize }); }
  onAction(action: string, row: any): void { this.actionClick.emit({ action, row }); }
  onRowClick(row: any): void { if (this.rowClick.observed) this.rowClick.emit(row); }
  toggleRow(row: any): void { if (this.selection.has(row)) this.selection.delete(row); else this.selection.add(row); this.selectionChange.emit(Array.from(this.selection)); }
  toggleAllRows(): void { if (this.isAllSelected()) this.selection.clear(); else this.dataSource.data.forEach(row => this.selection.add(row)); this.selectionChange.emit(Array.from(this.selection)); }
  isAllSelected(): boolean { return this.selection.size === this.dataSource.data.length && this.dataSource.data.length > 0; }
  getCellValue(row: any, key: string): any { return key.split('.').reduce((obj: any, k: string) => obj?.[k], row); }
  getBadgeColor(row: any, col: TableColumn): string { const v = this.getCellValue(row, col.key); return (col.badgeColors && col.badgeColors[v]) ? col.badgeColors[v] : '#e0e0e0'; }
  getBadgeTextColor(row: any, col: TableColumn): string { const bg = this.getBadgeColor(row, col); return ['#fbc02d', '#ffeb3b', '#fff176'].includes(bg) ? '#212121' : '#ffffff'; }
  getChipColor(row: any, col: TableColumn): string { const v = this.getCellValue(row, col.key); return (col.chipColors && col.chipColors[v]) ? col.chipColors[v] : '#757575'; }
}