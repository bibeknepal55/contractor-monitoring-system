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
import { TimeExtensionService } from '../../../core/services/time-extension.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';

@Component({
  selector: 'app-time-extension-list', standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule, MatTableModule, MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule, MatProgressBarModule, MatTooltipModule, FormsModule],
  template: `
    <div class="page">
      <div class="header"><div class="hl"><h1>Time Extensions</h1><span class="count">{{total}} requests</span></div>
        <button mat-flat-button color="primary" routerLink="new" *ngIf="auth.hasPermission('TimeExtension.Create')"><mat-icon>add</mat-icon> Request Extension</button></div>
      <div class="tb"><mat-form-field appearance="outline" class="search"><mat-icon matPrefix>search</mat-icon><input matInput [(ngModel)]="s" (ngModelChange)="onS($event)" placeholder="Search..."><button mat-icon-button matSuffix *ngIf="s" (click)="s='';onS('')"><mat-icon>close</mat-icon></button></mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh"><mat-icon>refresh</mat-icon></button></div>
      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>
      <div class="tc" *ngIf="!loading && items.length>0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)">
          <ng-container matColumnDef="project"><th mat-header-cell *matHeaderCellDef>Project</th><td mat-cell *matCellDef="let r" class="nc">{{r.projectName||(r.projectId|slice:0:8)||'-'}}</td></ng-container>
          <ng-container matColumnDef="requestDate"><th mat-header-cell *matHeaderCellDef mat-sort-header>Request Date</th><td mat-cell *matCellDef="let r">{{r.requestDate?fd(r.requestDate):'-'}}</td></ng-container>
          <ng-container matColumnDef="daysRequested"><th mat-header-cell *matHeaderCellDef mat-sort-header>Days</th><td mat-cell *matCellDef="let r"><span class="chip" [style.background]="(r.daysRequested||0)>30?'#fce8e6':(r.daysRequested||0)>15?'#fef7e0':'#e6f4ea'" [style.color]="(r.daysRequested||0)>30?'#c5221f':(r.daysRequested||0)>15?'#e65100':'#137333'">{{r.daysRequested||0}} days</span></td></ng-container>
          <ng-container matColumnDef="originalCompletionDate"><th mat-header-cell *matHeaderCellDef>Original Completion</th><td mat-cell *matCellDef="let r">{{r.originalCompletionDate?fd(r.originalCompletionDate):'-'}}</td></ng-container>
          <ng-container matColumnDef="newCompletionDate"><th mat-header-cell *matHeaderCellDef>New Completion</th><td mat-cell *matCellDef="let r" style="font-weight:600;">{{getNewDate(r)}}</td></ng-container>
          <ng-container matColumnDef="reason"><th mat-header-cell *matHeaderCellDef>Reason</th><td mat-cell *matCellDef="let r" style="max-width:200px;overflow:hidden;text-overflow:ellipsis;">{{r.reason||'-'}}</td></ng-container>
          <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef style="text-align:center;">Actions</th><td mat-cell *matCellDef="let r" class="act"><button mat-icon-button color="primary" (click)="edit(r)" *ngIf="auth.hasPermission('TimeExtension.Update')"><mat-icon>edit</mat-icon></button><button mat-icon-button color="warn" (click)="del(r)" *ngIf="auth.hasPermission('TimeExtension.Delete')"><mat-icon>delete</mat-icon></button></td></ng-container>
          <tr mat-header-row *matHeaderRowDef="c"></tr><tr mat-row *matRowDef="let r;columns:c" class="row"></tr>
        </table>
        <mat-paginator [length]="total" [pageSize]="ps" [pageIndex]="p-1" [pageSizeOptions]="[5,10,25]" (page)="onPg($event)" *ngIf="total>5"></mat-paginator>
      </div>
      <div class="empty" *ngIf="!loading && items.length===0"><mat-icon>schedule</mat-icon><h3>No Time Extensions</h3><button mat-flat-button color="primary" routerLink="new">Request Extension</button></div>
    </div>
  `,
  styles: [`.page{padding:24px;max-width:1400px;margin:0 auto}.header{display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;gap:12px}.hl h1{font-size:1.5rem;font-weight:700;margin:0}.count{font-size:.85rem;color:#666}.tb{display:flex;align-items:center;gap:8px;margin-bottom:16px}.search{width:320px}.tc{background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.06);overflow:hidden}table{width:100%}th{background:#fafafa;font-weight:600;font-size:.78rem;text-transform:uppercase;color:#555;padding:14px 16px}td{padding:14px 16px;font-size:.88rem}.row:hover td{background:#f8f9ff}.nc{font-weight:500}.chip{display:inline-block;padding:3px 12px;border-radius:20px;font-size:.75rem;font-weight:600}.act{text-align:center}.empty{text-align:center;padding:60px 20px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;margin-bottom:12px;color:#ccc}@media(max-width:768px){.page{padding:16px}.search{width:100%}}`]
})
export class TimeExtensionListComponent implements OnInit {
  private srv=inject(TimeExtensionService); readonly auth=inject(AuthService); private notify=inject(NotificationService); private router=inject(Router);
  items:any[]=[]; total=0; p=1; ps=10; sb='createdAt'; sd:'asc'|'desc'='desc'; s=''; loading=false; private sbj=new Subject<string>();
  c=['project','requestDate','daysRequested','originalCompletionDate','newCompletionDate','reason','actions'];
  ngOnInit(){this.sbj.pipe(debounceTime(300)).subscribe(()=>{this.p=1;this.fetch()});this.fetch()}
  fetch(){this.loading=true;this.srv.getAll({page:this.p,pageSize:this.ps,search:this.s||undefined,sortBy:this.sb,sortOrder:this.sd}).subscribe({next:(r:any)=>{if(r.success){this.items=r.data;this.total=r.totalCount;this.applyPending()}this.loading=false},error:()=>{this.loading=false}})}
  applyPending(){const c=sessionStorage.getItem('te_updated');if(!c)return;try{const u=JSON.parse(c);const i=this.items.findIndex((x:any)=>x.id===u.id);if(i>=0)this.items[i]={...this.items[i],...u};sessionStorage.removeItem('te_updated')}catch(e){}}
  getNewDate(r:any):string{if(!r.originalCompletionDate||!r.daysRequested)return'-';return moment(r.originalCompletionDate).add(r.daysRequested,'days').format('DD/MM/YYYY')}
  fd(d:string):string{return moment(d).format('DD/MM/YYYY')}
  onS(v:string){this.s=v;this.sbj.next(v)} onSort(s:Sort){this.sb=s.active;this.sd=s.direction==='desc'?'desc':'asc';this.fetch()}
  onPg(e:PageEvent){this.p=e.pageIndex+1;this.ps=e.pageSize;this.fetch()} edit(r:any){this.router.navigate(['/time-extensions',r.id,'edit'])}
  async del(r:any){if(!await this.notify.confirmDelete('Extension request'))return;this.srv.delete(r.id).subscribe({next:(x:any)=>{if(x.success){this.notify.success('Deleted');this.fetch()}else this.notify.error(x.message||'Failed')},error:()=>this.notify.error('Failed')})}
}