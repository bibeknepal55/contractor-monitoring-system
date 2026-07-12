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
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { BondService } from '../../../core/services/bond.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-bond-list', standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule, MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule, MatProgressBarModule, MatTooltipModule, FormsModule],
  template: `
    <div class="page">
      <div class="header"><div class="hl"><h1>Performance Bonds</h1><span class="count">{{total}} bonds</span></div>
        <button mat-flat-button color="primary" routerLink="new" *ngIf="auth.hasPermission('PerformanceBond.Create')"><mat-icon>add</mat-icon> Issue Bond</button></div>
      <div class="tb"><mat-form-field appearance="outline" class="search"><mat-icon matPrefix>search</mat-icon><input matInput [(ngModel)]="s" (ngModelChange)="onS($event)" placeholder="Search..."><button mat-icon-button matSuffix *ngIf="s" (click)="s='';onS('')"><mat-icon>close</mat-icon></button></mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh"><mat-icon>refresh</mat-icon></button></div>
      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>
      <div class="tc" *ngIf="!loading && items.length>0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)">
          <ng-container matColumnDef="project"><th mat-header-cell *matHeaderCellDef>Project</th><td mat-cell *matCellDef="let b">{{b.projectName||(b.projectId|slice:0:8)||'-'}}</td></ng-container>
          <ng-container matColumnDef="bondNumber"><th mat-header-cell *matHeaderCellDef>Bond #</th><td mat-cell *matCellDef="let b"><strong>{{b.bondNumber||'-'}}</strong></td></ng-container>
          <ng-container matColumnDef="bondAmount"><th mat-header-cell *matHeaderCellDef mat-sort-header>Amount</th><td mat-cell *matCellDef="let b">₹{{(b.bondAmount||0)|number:'1.0-0'}}</td></ng-container>
          <ng-container matColumnDef="bondType"><th mat-header-cell *matHeaderCellDef>Type</th><td mat-cell *matCellDef="let b"><span class="chip" [style.background]="tc(b.bondType)+'22'" [style.color]="tc(b.bondType)">{{b.bondType||'Performance'}}</span></td></ng-container>
          <ng-container matColumnDef="issuingBank"><th mat-header-cell *matHeaderCellDef>Bank</th><td mat-cell *matCellDef="let b">{{b.issuingBank||'-'}}</td></ng-container>
          <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let b"><span class="chip" [style.background]="sc(b)+'22'" [style.color]="sc(b)">{{st(b)}}</span></td></ng-container>
          <ng-container matColumnDef="dates"><th mat-header-cell *matHeaderCellDef>Issue → Expiry</th><td mat-cell *matCellDef="let b" class="dates"><div>{{b.issueDate?fd(b.issueDate):'-'}}</div><div [class.red]="ix(b)" [class.orange]="is(b)">→ {{b.expiryDate?fd(b.expiryDate):'-'}}</div></td></ng-container>
          <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef style="text-align:center;">Actions</th><td mat-cell *matCellDef="let b" class="act"><button mat-icon-button color="primary" (click)="edit(b)" *ngIf="auth.hasPermission('PerformanceBond.Update')"><mat-icon>edit</mat-icon></button><button mat-icon-button color="warn" (click)="del(b)" *ngIf="auth.hasPermission('PerformanceBond.Delete')"><mat-icon>delete</mat-icon></button></td></ng-container>
          <tr mat-header-row *matHeaderRowDef="c"></tr><tr mat-row *matRowDef="let r;columns:c" class="row"></tr>
        </table>
        <mat-paginator [length]="total" [pageSize]="ps" [pageIndex]="p-1" [pageSizeOptions]="[5,10,25]" (page)="onPg($event)" *ngIf="total>5"></mat-paginator>
      </div>
      <div class="empty" *ngIf="!loading && items.length===0"><mat-icon>verified_user</mat-icon><h3>No Bonds</h3><button mat-flat-button color="primary" routerLink="new">Issue Bond</button></div>
    </div>
  `,
  styles: [`.page{padding:24px;max-width:1400px;margin:0 auto}.header{display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;gap:12px}.hl h1{font-size:1.5rem;font-weight:700;margin:0}.count{font-size:.85rem;color:#666}.tb{display:flex;align-items:center;gap:8px;margin-bottom:16px}.search{width:320px}.tc{background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.06);overflow:hidden}table{width:100%}th{background:#fafafa;font-weight:600;font-size:.78rem;text-transform:uppercase;color:#555;padding:14px 16px}td{padding:14px 16px;font-size:.88rem}.row:hover td{background:#f8f9ff}.chip{display:inline-block;padding:3px 12px;border-radius:20px;font-size:.75rem;font-weight:600}.dates{line-height:1.5;font-size:.82rem}.red{color:#c5221f;font-weight:600}.orange{color:#e65100;font-weight:600}.act{text-align:center}.empty{text-align:center;padding:60px 20px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;margin-bottom:12px;color:#ccc}@media(max-width:768px){.page{padding:16px}.search{width:100%}}`]
})
export class BondListComponent implements OnInit {
  private srv = inject(BondService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);
  private router = inject(Router);

  items: any[] = [];
  total = 0;
  p = 1;
  ps = 10;
  sb = 'createdAt';
  sd: 'asc' | 'desc' = 'desc';
  s = '';
  loading = false;
  private sbj = new Subject<string>();
  c = ['project', 'bondNumber', 'bondAmount', 'bondType', 'issuingBank', 'status', 'dates', 'actions'];

  ngOnInit(): void {
    this.sbj.pipe(debounceTime(300)).subscribe(() => { this.p = 1; this.fetch(); });
    this.fetch();
  }

  fetch(): void {
    this.loading = true;
    this.srv.getAll({ page: this.p, pageSize: this.ps, search: this.s || undefined, sortBy: this.sb, sortOrder: this.sd }).subscribe({
      next: (r: any) => {
        if (r.success) {
          this.items = r.data;
          this.total = r.totalCount;
          // CHECK sessionStorage for recently updated bond and MERGE it
          this.applyPendingUpdate();
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  // THIS IS THE KEY FIX: Apply the updated data from sessionStorage
  applyPendingUpdate(): void {
    const cached = sessionStorage.getItem('bond_updated');
    if (!cached) return;
    try {
      const updated = JSON.parse(cached);
      const idx = this.items.findIndex((i: any) => i.id === updated.id);
      if (idx >= 0) {
        this.items[idx] = { ...this.items[idx], ...updated };
      }
      sessionStorage.removeItem('bond_updated');
    } catch (e) { /* ignore */ }
  }

  tc(t: string): string { const m: any = { 'Performance': '#1976d2', 'Bid': '#388e3c', 'Retention': '#f57c00', 'Advance': '#7b1fa2' }; return m[t] || '#757575'; }
  st(b: any): string { if (!b.expiryDate) return 'Active'; return moment(b.expiryDate).isBefore(moment()) ? 'Expired' : moment(b.expiryDate).diff(moment(), 'days') <= 30 ? 'Expiring' : 'Active'; }
  sc(b: any): string { if (!b.expiryDate) return '#137333'; return moment(b.expiryDate).isBefore(moment()) ? '#c5221f' : moment(b.expiryDate).diff(moment(), 'days') <= 30 ? '#e65100' : '#137333'; }
  ix(b: any): boolean { return b.expiryDate && moment(b.expiryDate).isBefore(moment()); }
  is(b: any): boolean { return b.expiryDate && !moment(b.expiryDate).isBefore(moment()) && moment(b.expiryDate).diff(moment(), 'days') <= 30; }
  fd(d: string): string { return moment(d).format('DD/MM/YYYY'); }
  onS(v: string): void { this.s = v; this.sbj.next(v); }
  onSort(s: Sort): void { this.sb = s.active; this.sd = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPg(e: PageEvent): void { this.p = e.pageIndex + 1; this.ps = e.pageSize; this.fetch(); }
  edit(b: any): void { this.router.navigate(['/bonds', b.id, 'edit']); }

  async del(b: any): Promise<void> {
    if (!await this.notify.confirmDelete('Bond ' + b.bondNumber)) return;
    this.srv.delete(b.id).subscribe({
      next: (r: any) => { if (r.success) { this.notify.success('Deleted'); this.fetch(); } else this.notify.error(r.message || 'Failed'); },
      error: () => this.notify.error('Failed')
    });
  }
}