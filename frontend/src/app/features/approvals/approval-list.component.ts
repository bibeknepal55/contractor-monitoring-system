import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialogModule } from '@angular/material/dialog';
import { ApprovalService } from '../../core/services/approval.service';
import { ProjectService } from '../../core/services/project.service';
import { ContractorService } from '../../core/services/contractor.service';
import { BondService } from '../../core/services/bond.service';
import { GuaranteeService } from '../../core/services/guarantee.service';
import { PriceAdjustmentService } from '../../core/services/price-adjustment.service';
import { TimeExtensionService } from '../../core/services/time-extension.service';
import { SubcontractorService } from '../../core/services/subcontractor.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { debounceTime, Subject } from 'rxjs';
import moment from 'moment';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-approval-list', standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatButtonModule, MatIconModule, MatTableModule, MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressBarModule, MatTooltipModule, MatCardModule, MatDividerModule, MatDialogModule],
  template: `
    <div class="page">
      <div class="top-bar">
        <div><h1>Approval Workflow</h1><span class="subtitle">{{total}} requests</span></div>
        <button mat-flat-button color="primary" (click)="openNewRequest()" *ngIf="auth.hasPermission('ApprovalWorkflow.Create')">
          <mat-icon>add</mat-icon> New Request
        </button>
      </div>

      <mat-card class="form-card" *ngIf="showForm">
        <mat-card-header>
          <mat-icon mat-card-avatar style="color:#1a73e8;">send</mat-icon>
          <mat-card-title>{{ editingId ? 'Edit Request' : 'Submit for Approval' }}</mat-card-title>
        </mat-card-header>
        <mat-divider></mat-divider>
        <mat-card-content>
          <form [formGroup]="submitForm" (ngSubmit)="doSubmit()">
            <div class="g2">
              <mat-form-field appearance="outline"><mat-label>Module</mat-label><mat-select formControlName="moduleName" (selectionChange)="loadRecords()">
                <mat-option value="Project">📋 Project</mat-option>
                <mat-option value="Contractor">🏢 Contractor</mat-option>
                <mat-option value="PriceAdjustment">💰 Price Adjustment</mat-option>
                <mat-option value="TimeExtension">⏰ Time Extension</mat-option>
                <mat-option value="PerformanceBond">🔒 Performance Bond</mat-option>
                <mat-option value="AdvancePaymentGuarantee">🛡️ APG</mat-option>
                <mat-option value="Subcontractor">🤝 Subcontractor</mat-option>
              </mat-select></mat-form-field>
              <mat-form-field appearance="outline"><mat-label>Level</mat-label><mat-select formControlName="approvalLevel"><mat-option [value]="1">Level 1</mat-option><mat-option [value]="2">Level 2</mat-option><mat-option [value]="3">Level 3</mat-option></mat-select></mat-form-field>
            </div>

            <div *ngIf="loadingRecords" style="padding:12px 0;text-align:center;"><mat-progress-bar mode="indeterminate"></mat-progress-bar><small>Loading records...</small></div>

            <mat-form-field appearance="outline" *ngIf="records.length>0&&!loadingRecords"><mat-label>Record</mat-label><mat-select formControlName="recordId"><mat-option *ngFor="let r of records" [value]="r.id">{{r.name}}</mat-option></mat-select></mat-form-field>

            <div *ngIf="records.length===0&&!loadingRecords&&submitForm.get('moduleName')?.value" style="padding:8px 0;">
              <mat-form-field appearance="outline"><mat-label>Record ID (GUID)</mat-label><input matInput formControlName="recordId" placeholder="Paste a valid GUID"><mat-hint>No records loaded for this module</mat-hint></mat-form-field>
            </div>

            <mat-form-field appearance="outline"><mat-label>Comments</mat-label><textarea matInput formControlName="comments" rows="2" placeholder="Describe what needs approval (e.g., low manpower, budget increase)"></textarea></mat-form-field>
            <div class="btns"><button mat-stroked-button type="button" (click)="cancelForm()">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="submitting||submitForm.invalid">{{ editingId ? 'Update Request' : 'Submit Request' }}</button></div>
          </form>
        </mat-card-content>
      </mat-card>

      <div class="filter-bar" *ngIf="!showForm">
        <mat-form-field appearance="outline" class="sf"><mat-icon matPrefix>search</mat-icon><input matInput [(ngModel)]="searchText" (ngModelChange)="onSearch($event)" placeholder="Search..."></mat-form-field>
        <mat-form-field appearance="outline" style="width:150px"><mat-label>Status</mat-label><mat-select [(ngModel)]="statusFilter" (selectionChange)="fetch()"><mat-option value="">All</mat-option><mat-option value="Pending">Pending</mat-option><mat-option value="Approved">Approved</mat-option><mat-option value="Rejected">Rejected</mat-option></mat-select></mat-form-field>
        <button mat-icon-button (click)="fetch()" matTooltip="Refresh"><mat-icon>refresh</mat-icon></button>
      </div>

      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary"></mat-progress-bar>

      <div class="table-wrap" *ngIf="!loading && !showForm && items.length>0">
        <table mat-table [dataSource]="items" matSort (matSortChange)="onSort($event)">
          <ng-container matColumnDef="comments">
            <th mat-header-cell *matHeaderCellDef>Request Details</th>
            <td mat-cell *matCellDef="let r">
              <strong>{{ getRequestComment(r) || 'No comments' }}</strong>
              <br><small style="color:#999;">{{fmtDate(r.createdAt)}}</small>
            </td>
          </ng-container>
          <ng-container matColumnDef="approvalLevel"><th mat-header-cell *matHeaderCellDef>Level</th><td mat-cell *matCellDef="let r" style="text-align:center;"><span class="lvl">L{{r.approvalLevel||1}}</span></td></ng-container>
          <ng-container matColumnDef="requestedBy"><th mat-header-cell *matHeaderCellDef>Requested By</th><td mat-cell *matCellDef="let r">{{r.requestedBy||'-'}}</td></ng-container>
          <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let r"><span class="badge" [class.ok]="r.status==='Approved'" [class.pend]="r.status==='Pending'" [class.rej]="r.status==='Rejected'">{{r.status||'Pending'}}</span></td></ng-container>
          <ng-container matColumnDef="result">
            <th mat-header-cell *matHeaderCellDef>Result</th>
            <td mat-cell *matCellDef="let r">
              <div *ngIf="r.status==='Approved'" class="result-row approved">
                <mat-icon class="result-icon">check_circle</mat-icon>
                <div>
                  <span class="result-text">Approved by <strong>{{r.approvedBy||'Admin'}}</strong></span>
                  <small class="result-date">{{fmtDate(r.approvedAt||r.updatedAt)}}</small>
                  <small *ngIf="r.approvalComments" class="result-comment">"{{r.approvalComments}}"</small>
                </div>
              </div>
              <div *ngIf="r.status==='Rejected'" class="result-row rejected">
                <mat-icon class="result-icon">cancel</mat-icon>
                <div>
                  <span class="result-text">Rejected by <strong>{{r.rejectedBy||'Admin'}}</strong></span>
                  <small class="result-date">{{fmtDate(r.rejectedAt||r.updatedAt)}}</small>
                  <small *ngIf="r.approvalComments" class="result-comment">Reason: "{{r.approvalComments}}"</small>
                </div>
              </div>
              <span *ngIf="r.status==='Pending'" class="pending-msg">
                <mat-icon style="font-size:16px;width:16px;height:16px;vertical-align:middle;color:#e65100;">hourglass_empty</mat-icon>
                Awaiting approval
              </span>
            </td>
          </ng-container>
          <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef style="text-align:center;">Actions</th><td mat-cell *matCellDef="let r" style="text-align:center;">
            <ng-container *ngIf="r.status==='Pending' && isOwner(r)">
              <button mat-icon-button color="primary" (click)="editRequest(r)" matTooltip="Edit request" style="margin-right:4px;">
                <mat-icon>edit</mat-icon>
              </button>
            </ng-container>
            <ng-container *ngIf="r.status==='Pending' && canApprove">
              <button mat-raised-button color="primary" (click)="approve(r)" style="margin-right:4px;"><mat-icon>check</mat-icon> Approve</button>
              <button mat-raised-button color="warn" (click)="reject(r)"><mat-icon>close</mat-icon> Reject</button>
            </ng-container>
            <span *ngIf="r.status!=='Pending' || (!canApprove && !isOwner(r))" style="color:#999;">—</span>
          </td></ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr><tr mat-row *matRowDef="let r;columns:cols;"></tr>
        </table>
        <mat-paginator [length]="total" [pageSize]="ps" [pageIndex]="p-1" [pageSizeOptions]="[5,10,25]" (page)="onPg($event)" *ngIf="total>5"></mat-paginator>
      </div>

      <div class="empty" *ngIf="!loading && !showForm && items.length===0"><mat-icon>fact_check</mat-icon><h3>No approval requests</h3><p>Click "New Request" to submit one</p></div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1400px;margin:0 auto}
    .top-bar{display:flex;justify-content:space-between;align-items:center;margin-bottom:24px}
    .top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.subtitle{color:#666;font-size:.85rem}
    .form-card{border-radius:12px;border:1px solid #e0e0e0;margin-bottom:24px;box-shadow:0 2px 12px rgba(0,0,0,0.06)}
    .form-card mat-card-header{padding:20px 24px 0}.form-card mat-card-content{padding:16px 24px 24px}
    mat-divider{margin:12px 24px}
    .g2{display:grid;grid-template-columns:1fr 1fr;gap:16px}.btns{display:flex;justify-content:flex-end;gap:12px;margin-top:12px}
    .filter-bar{display:flex;align-items:center;gap:8px;margin-bottom:16px}.sf{width:300px}
    .table-wrap{background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.06);overflow:hidden}
    table{width:100%}th{background:#fafafa;font-weight:600;font-size:.78rem;text-transform:uppercase;color:#555;padding:14px 16px}td{padding:14px 16px;font-size:.88rem}
    tr:hover td{background:#f8f9ff}
    .lvl{display:inline-block;padding:2px 10px;background:#ede7f6;color:#5c6bc0;border-radius:12px;font-weight:700;font-size:.8rem}
    .badge{display:inline-block;padding:4px 14px;border-radius:20px;font-size:.75rem;font-weight:600}
    .badge.ok{background:#e6f4ea;color:#137333}.badge.pend{background:#fff3e0;color:#e65100}.badge.rej{background:#fce8e6;color:#c5221f}
    .empty{text-align:center;padding:64px;color:#888}.empty mat-icon{font-size:56px;width:56px;height:56px;color:#ccc}
    mat-form-field{width:100%}
    .result-row{display:flex;align-items:flex-start;gap:8px}
    .result-icon{font-size:18px;width:18px;height:18px;margin-top:1px;flex-shrink:0}
    .approved .result-icon{color:#137333}.rejected .result-icon{color:#c5221f}
    .result-text{display:block;font-size:0.82rem;color:#333;font-weight:500}
    .result-date{display:block;font-size:0.72rem;color:#999}
    .result-comment{display:block;font-size:0.72rem;color:#666;font-style:italic;margin-top:2px}
    .pending-msg{font-size:0.8rem;color:#e65100;font-weight:500;display:inline-flex;align-items:center;gap:4px}
    @media(max-width:768px){.page{padding:16px}.g2{grid-template-columns:1fr}.sf{width:100%}}
  `]
})
export class ApprovalListComponent implements OnInit {
  private srv=inject(ApprovalService); readonly auth=inject(AuthService); private notify=inject(NotificationService); private fb=inject(FormBuilder);
  private pSrv=inject(ProjectService); private cSrv=inject(ContractorService); private bondSrv=inject(BondService);
  private apgSrv=inject(GuaranteeService); private paSrv=inject(PriceAdjustmentService); private teSrv=inject(TimeExtensionService); private subSrv=inject(SubcontractorService);

  items:any[]=[]; total=0; p=1; ps=10; sb='createdAt'; sd:'asc'|'desc'='desc'; searchText=''; statusFilter=''; loading=false; showForm=false; submitting=false; records:any[]=[]; loadingRecords=false; editingId:string|null=null;
  private search$=new Subject<string>();
  cols=['comments','approvalLevel','requestedBy','status','result','actions'];
  submitForm=this.fb.group({moduleName:['',Validators.required],recordId:['',Validators.required],comments:[''],approvalLevel:[1]});

  get canApprove(): boolean { return this.auth.hasAnyRole(['SuperAdmin', 'Admin']); }

  ngOnInit(){this.search$.pipe(debounceTime(300)).subscribe(()=>{this.p=1;this.fetch()});this.fetch()}

  fetch(){
    this.loading=true;
    const params:any={page:this.p,pageSize:this.ps,search:this.searchText||undefined,sortBy:this.sb,sortOrder:this.sd};
    if(this.statusFilter)params.status=this.statusFilter;
    this.srv.getAll(params).subscribe({next:(r:any)=>{if(r.success){this.items=r.data;this.total=r.totalCount}this.loading=false},error:()=>this.loading=false});
  }

  getRequestComment(r: any): string {
    return r.comments || r.requestComments || r.description || r.comment || '';
  }

  isOwner(r: any): boolean {
    const currentUser = this.auth.getCurrentUser();
    if (!currentUser) return false;
    const currentUserName = `${currentUser.firstName} ${currentUser.lastName}`.trim();
    return r.requestedBy === currentUserName || r.userId === currentUser.id;
  }

  openNewRequest(): void {
    this.editingId = null;
    this.submitForm.reset({approvalLevel:1});
    this.records = [];
    this.showForm = true;
  }

  editRequest(r: any): void {
    this.editingId = r.id;
    this.submitForm.patchValue({
      moduleName: r.moduleName || '',
      recordId: r.recordId || '',
      comments: this.getRequestComment(r),
      approvalLevel: r.approvalLevel || 1
    });
    if (r.moduleName) this.loadRecords();
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.submitForm.reset({approvalLevel:1});
    this.records = [];
  }

  loadRecords(){
    const mod=this.submitForm.get('moduleName')?.value;this.records=[];this.submitForm.patchValue({recordId:''});if(!mod)return;
    this.loadingRecords=true;const req={page:1,pageSize:200};let obs;
    switch(mod){case'Project':obs=this.pSrv.getProjects(req);break;case'Contractor':obs=this.cSrv.getContractors(req);break;case'PriceAdjustment':obs=this.paSrv.getAll(req);break;case'TimeExtension':obs=this.teSrv.getAll(req);break;case'PerformanceBond':obs=this.bondSrv.getAll(req);break;case'AdvancePaymentGuarantee':obs=this.apgSrv.getAll(req);break;case'Subcontractor':obs=this.subSrv.getAll(req);break;default:this.loadingRecords=false;return}
    obs.subscribe({next:(resp:any)=>{this.loadingRecords=false;if(resp?.data?.length>0){this.records=resp.data.map((item:any)=>({id:item.id,name:item.projectName||item.companyName||item.bondNumber||item.guaranteeNumber||item.fullName||item.testName||item.materialName||item.title||item.name||(item.id?item.id.substring(0,8)+'...':'Unknown')}))}else{this.notify.warning('No '+mod+' records found')}},error:()=>{this.loadingRecords=false;this.notify.error('Failed to load')}});
  }

  doSubmit(){
    if(this.submitForm.invalid){this.submitForm.markAllAsTouched();return}
    this.submitting=true;
    const v=this.submitForm.getRawValue();
    const sel=this.records.find(r=>r.id===v.recordId);
    const body={moduleName:v.moduleName,recordId:v.recordId,recordTitle:sel?.name||v.recordId||'',comments:v.comments||'',approvalLevel:Number(v.approvalLevel)};

    if (this.editingId) {
      // Update existing request using the new backend endpoint
      this.srv.update(this.editingId, body).subscribe({
        next:(r:any)=>{
          this.submitting=false;
          if(r.success){
            this.notify.success('Request updated!');
            this.cancelForm();
            this.fetch();
          } else {
            this.notify.error(r.message||'Failed to update');
          }
        },
        error:(e:any)=>{
          this.submitting=false;
          this.notify.error(e?.error?.message||'Failed to update');
        }
      });
    } else {
      // Create new request
      this.srv.submit({request:body}).subscribe({
        next:(r:any)=>{
          this.submitting=false;
          if(r.success){
            this.notify.success('Submitted!');
            this.cancelForm();
            this.fetch();
          } else {
            this.notify.error(r.message||'Failed');
          }
        },
        error:(e:any)=>{
          this.submitting=false;
          this.notify.error(e?.error?.message||'Failed');
        }
      });
    }
  }

  async approve(r:any){const c=await this.notify.showPrompt('Approve','Comments');if(c===null)return;this.srv.process(r.id,{action:'Approved',comments:c}).subscribe({next:(x:any)=>{if(x.success){this.notify.success('Approved!');this.fetch()}else this.notify.error(x.message||'Failed')},error:()=>this.notify.error('Failed')})}
  async reject(r:any){const c=await this.notify.showPrompt('Reject','Reason');if(c===null)return;this.srv.process(r.id,{action:'Rejected',comments:c}).subscribe({next:(x:any)=>{if(x.success){this.notify.success('Rejected');this.fetch()}else this.notify.error(x.message||'Failed')},error:()=>this.notify.error('Failed')})}

  fmtDate(d:string):string{return d?moment(d).format('DD/MM/YYYY HH:mm'):'-'}
  onSearch(v:string){this.searchText=v;this.search$.next(v)}
  onSort(s:Sort){this.sb=s.active;this.sd=s.direction==='desc'?'desc':'asc';this.fetch()}
  onPg(e:PageEvent){this.p=e.pageIndex+1;this.ps=e.pageSize;this.fetch()}
}