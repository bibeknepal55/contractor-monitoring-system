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
import { GuaranteeService } from '../../../core/services/guarantee.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-guarantee-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule,
    MatProgressBarModule, MatTooltipModule, FormsModule,
  ],
  template: `
    <div class="page">
      <div class="header">
        <div class="hl"><h1>Advance Payment Guarantees</h1><span class="count">{{total}} guarantees</span></div>
        <button mat-flat-button color="primary" routerLink="new" *ngIf="auth.hasPermission('AdvancePaymentGuarantee.Create')"><mat-icon>add</mat-icon> Issue APG</button>
      </div>
      <div class="tb">
        <mat-form-field appearance="outline" class="search"><mat-icon matPrefix>search</mat-icon><input matInput [(ngModel)]="s" (ngModelChange)="onS($event)" placeholder="Search..."><button mat-icon-button matSuffix *ngIf="s" (click)="s='';onS('')"><mat-icon>close</mat-icon></button></mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh"><mat-icon>refresh</mat-icon></button>
      </div>
      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>
      <div class="tc" *ngIf="!loading && items.length>0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)">
          <ng-container matColumnDef="project"><th mat-header-cell *matHeaderCellDef>Project</th><td mat-cell *matCellDef="let g">{{g.projectName||(g.projectId|slice:0:8)||'-'}}</td></ng-container>
          <ng-container matColumnDef="guaranteeNumber"><th mat-header-cell *matHeaderCellDef>APG #</th><td mat-cell *matCellDef="let g"><strong>{{g.guaranteeNumber||'-'}}</strong></td></ng-container>
          <ng-container matColumnDef="guaranteeAmount"><th mat-header-cell *matHeaderCellDef mat-sort-header>Guarantee</th><td mat-cell *matCellDef="let g">₹{{(g.guaranteeAmount||0)|number:'1.0-0'}}</td></ng-container>
          <ng-container matColumnDef="advanceAmount"><th mat-header-cell *matHeaderCellDef mat-sort-header>Advance</th><td mat-cell *matCellDef="let g">₹{{(g.advanceAmount||0)|number:'1.0-0'}}</td></ng-container>
          <ng-container matColumnDef="issuingBank"><th mat-header-cell *matHeaderCellDef>Bank</th><td mat-cell *matCellDef="let g">{{g.issuingBank||'-'}}</td></ng-container>
          <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let g"><span class="chip" [style.background]="sc(g)+'22'" [style.color]="sc(g)">{{st(g)}}</span></td></ng-container>
          <ng-container matColumnDef="dates"><th mat-header-cell *matHeaderCellDef>Issue → Expiry</th><td mat-cell *matCellDef="let g" class="dates"><div>{{g.issueDate?fd(g.issueDate):'-'}}</div><div [class.red]="ix(g)" [class.orange]="is(g)">→ {{g.expiryDate?fd(g.expiryDate):'-'}}</div></td></ng-container>
          <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef style="text-align:center;">Actions</th><td mat-cell *matCellDef="let g" class="act"><button mat-icon-button color="primary" (click)="edit(g)" *ngIf="auth.hasPermission('AdvancePaymentGuarantee.Update')"><mat-icon>edit</mat-icon></button><button mat-icon-button color="warn" (click)="del(g)" *ngIf="auth.hasPermission('AdvancePaymentGuarantee.Delete')"><mat-icon>delete</mat-icon></button></td></ng-container>
          <tr mat-header-row *matHeaderRowDef="c"></tr><tr mat-row *matRowDef="let r;columns:c" class="row"></tr>
        </table>
        <mat-paginator [length]="total" [pageSize]="ps" [pageIndex]="p-1" [pageSizeOptions]="[5,10,25]" (page)="onPg($event)" *ngIf="total>5" showFirstLastButtons></mat-paginator>
      </div>
      <div class="empty" *ngIf="!loading && items.length===0"><mat-icon>shield</mat-icon><h3>No APGs</h3><p>Issue a bank guarantee for advance payment</p><button mat-flat-button color="primary" routerLink="new">Issue First APG</button></div>
    </div>
  `,
  styles: [`.page{padding:24px;max-width:1400px;margin:0 auto}.header{display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;gap:12px}.hl h1{font-size:1.5rem;font-weight:700;margin:0}.count{font-size:.85rem;color:#666}.tb{display:flex;align-items:center;gap:8px;margin-bottom:16px}.search{width:320px}.tc{background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.06);overflow:hidden}table{width:100%}th{background:#fafafa;font-weight:600;font-size:.78rem;text-transform:uppercase;color:#555;padding:14px 16px;white-space:nowrap}td{padding:14px 16px;font-size:.88rem;color:#333;white-space:nowrap}.row:hover td{background:#f8f9ff}.chip{display:inline-block;padding:3px 12px;border-radius:20px;font-size:.75rem;font-weight:600}.dates{line-height:1.5;font-size:.82rem}.red{color:#c5221f;font-weight:600}.orange{color:#e65100;font-weight:600}.act{text-align:center}.empty{text-align:center;padding:60px 20px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;margin-bottom:12px;color:#ccc}.empty h3{margin:0 0 4px;color:#555}@media(max-width:768px){.page{padding:16px}.search{width:100%}}`]
})
export class GuaranteeListComponent implements OnInit {
  private srv = inject(GuaranteeService);
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
  c = ['project', 'guaranteeNumber', 'guaranteeAmount', 'advanceAmount', 'issuingBank', 'status', 'dates', 'actions'];

  ngOnInit(): void {
    this.sbj.pipe(debounceTime(300)).subscribe(() => { this.p = 1; this.fetch(); });
    this.fetch();
  }

  fetch(): void {
    this.loading = true;
    this.srv.getAll({
      page: this.p, pageSize: this.ps,
      search: this.s || undefined,
      sortBy: this.sb, sortOrder: this.sd
    }).subscribe({
      next: (r: any) => {
        if (r.success) {
          this.items = r.data;
          this.total = r.totalCount;
          // MERGE pending update from sessionStorage
          this.applyPendingUpdate();
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  // KEY FIX: Apply updated data from sessionStorage over API response
  applyPendingUpdate(): void {
    const cached = sessionStorage.getItem('apg_updated');
    if (!cached) return;
    try {
      const updated = JSON.parse(cached);
      const idx = this.items.findIndex((i: any) => i.id === updated.id);
      if (idx >= 0) {
        this.items[idx] = { ...this.items[idx], ...updated };
      }
      sessionStorage.removeItem('apg_updated');
    } catch (e) { /* ignore */ }
  }

  st(g: any): string {
    if (!g.expiryDate) return 'Active';
    if (moment(g.expiryDate).isBefore(moment())) return 'Expired';
    if (moment(g.expiryDate).diff(moment(), 'days') <= 30) return 'Expiring';
    return 'Active';
  }

  sc(g: any): string {
    if (!g.expiryDate) return '#137333';
    if (moment(g.expiryDate).isBefore(moment())) return '#c5221f';
    if (moment(g.expiryDate).diff(moment(), 'days') <= 30) return '#e65100';
    return '#137333';
  }

  ix(g: any): boolean { return g.expiryDate && moment(g.expiryDate).isBefore(moment()); }
  is(g: any): boolean {
    return g.expiryDate && !moment(g.expiryDate).isBefore(moment()) && moment(g.expiryDate).diff(moment(), 'days') <= 30;
  }

  fd(d: string): string { return moment(d).format('DD/MM/YYYY'); }
  onS(v: string): void { this.s = v; this.sbj.next(v); }
  onSort(s: Sort): void { this.sb = s.active; this.sd = s.direction === 'desc' ? 'desc' : 'asc'; this.fetch(); }
  onPg(e: PageEvent): void { this.p = e.pageIndex + 1; this.ps = e.pageSize; this.fetch(); }
  edit(g: any): void { this.router.navigate(['/guarantees', g.id, 'edit']); }

  async del(g: any): Promise<void> {
    if (!await this.notify.confirmDelete('APG ' + g.guaranteeNumber)) return;
    this.srv.delete(g.id).subscribe({
      next: (r: any) => {
        if (r.success) { this.notify.success('Deleted'); this.fetch(); }
        else this.notify.error(r.message || 'Failed');
      },
      error: () => this.notify.error('Failed')
    });
  }
}