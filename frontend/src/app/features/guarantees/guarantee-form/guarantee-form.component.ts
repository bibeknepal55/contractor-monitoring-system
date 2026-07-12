import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GuaranteeService } from '../../../core/services/guarantee.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-guarantee-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink,
  ],
  template: `
    <div class="form-page">
      <div class="top-bar">
        <button mat-icon-button routerLink="/guarantees"><mat-icon>arrow_back</mat-icon></button>
        <div><h1>{{edit?'Edit':'Issue'}} Advance Payment Guarantee</h1><p>Bank guarantee for advance payment security</p></div>
      </div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Guarantee</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Guarantee Number *</mat-label><input matInput formControlName="guaranteeNumber" placeholder="APG-2026-001"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Issuing Bank *</mat-label><input matInput formControlName="issuingBank" placeholder="Bank name"><mat-error>Required</mat-error></mat-form-field>
          </div>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Guarantee Amount *</mat-label><input matInput type="number" formControlName="guaranteeAmount" placeholder="0"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Advance Amount *</mat-label><input matInput type="number" formControlName="advanceAmount" placeholder="0"><mat-error>Required</mat-error></mat-form-field>
          </div>
        </mat-card-content></mat-card>
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>event</mat-icon><mat-card-title>Validity</mat-card-title></mat-card-header><mat-card-content>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Issue Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="issueDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Expiry Date *</mat-label><input matInput [matDatepicker]="d2" formControlName="expiryDate"><mat-datepicker-toggle matSuffix [for]="d2"></mat-datepicker-toggle><mat-datepicker #d2></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
          </div>
        </mat-card-content></mat-card>
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>notes</mat-icon><mat-card-title>Remarks</mat-card-title></mat-card-header><mat-card-content><mat-form-field appearance="outline"><mat-label>Remarks</mat-label><textarea matInput formControlName="remarks" rows="2"></textarea></mat-form-field></mat-card-content></mat-card>
        <div class="btns"><button mat-stroked-button type="button" routerLink="/guarantees">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Issue APG'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2{grid-template-columns:1fr}}`]
})
export class GuaranteeFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private srv = inject(GuaranteeService);
  private pSrv = inject(ProjectService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  projects: any[] = [];
  edit = false;
  id: string | null = null;
  saving = false;

  f = this.fb.group({
    projectId: ['', Validators.required],
    guaranteeNumber: ['', Validators.required],
    guaranteeAmount: [0, [Validators.required, Validators.min(0)]],
    advanceAmount: [0, [Validators.required, Validators.min(0)]],
    issuingBank: ['', Validators.required],
    issueDate: [null as Date | null, Validators.required],
    expiryDate: [null as Date | null, Validators.required],
    remarks: [''],
  });

  ngOnInit(): void {
    this.pSrv.getProjects({ page: 1, pageSize: 100 }).subscribe((r: any) => {
      if (r.success) this.projects = r.data;
    });
    const iid = this.route.snapshot.paramMap.get('id');
    if (iid) { this.edit = true; this.id = iid; this.load(iid); }
  }

  load(id: string): void {
    this.saving = true;
    this.srv.getById(id).subscribe({
      next: (r: any) => {
        if (r?.data) {
          const d = r.data;
          this.f.patchValue({
            projectId: d.projectId, guaranteeNumber: d.guaranteeNumber,
            guaranteeAmount: Number(d.guaranteeAmount) || 0, advanceAmount: Number(d.advanceAmount) || 0,
            issuingBank: d.issuingBank || '',
            issueDate: d.issueDate ? moment(d.issueDate).toDate() : null,
            expiryDate: d.expiryDate ? moment(d.expiryDate).toDate() : null,
            remarks: d.remarks || '',
          });
        }
        this.saving = false;
      },
      error: () => { this.saving = false; this.router.navigate(['/guarantees']); }
    });
  }

  submit(): void {
    if (this.f.invalid) { this.f.markAllAsTouched(); return; }
    this.saving = true;
    const v = this.f.getRawValue();
    const body: any = {
      projectId: v.projectId,
      guaranteeNumber: (v.guaranteeNumber || '').trim(),
      guaranteeAmount: Number(v.guaranteeAmount) || 0,
      advanceAmount: Number(v.advanceAmount) || 0,
      issuingBank: (v.issuingBank || '').trim(),
      issueDate: v.issueDate ? moment(v.issueDate).toISOString() : null,
      expiryDate: v.expiryDate ? moment(v.expiryDate).toISOString() : null,
      remarks: (v.remarks || '').trim(),
    };

    const r$ = this.edit && this.id ? this.srv.update(this.id, body) : this.srv.create(body);
    r$.subscribe({
      next: (r: any) => {
        this.saving = false;
        if (r.success) {
          // STORE updated data in sessionStorage so list can merge it
          const updatedData = { ...body, id: this.id || r.data?.id, projectName: r.data?.projectName || '' };
          sessionStorage.setItem('apg_updated', JSON.stringify(updatedData));
          this.notify.success(this.edit ? 'APG updated!' : 'APG issued!');
          this.router.navigate(['/guarantees']);
        } else {
          this.notify.error(r.message || 'Failed');
        }
      },
      error: (e: any) => { this.saving = false; this.notify.error(e?.error?.message || 'Failed'); }
    });
  }
}