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
import { FinancialService } from '../../../core/services/financial.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-financial-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink,
  ],
  template: `
    <div class="form-page">
      <div class="form-header">
        <button mat-icon-button routerLink="/financials" class="back-btn">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <div>
          <h1>{{ isEdit ? 'Edit Contract Financial' : 'New Contract Financial' }}</h1>
          <p>{{ isEdit ? 'Update financial details' : 'Add contract financial information' }}</p>
        </div>
      </div>

      <div class="form-body">
        <form [formGroup]="form" (ngSubmit)="save()">
          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>account_balance</mat-icon>
              <mat-card-title>Financial Details</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <mat-form-field appearance="outline">
                <mat-label>Project *</mat-label>
                <mat-select formControlName="projectId">
                  <mat-option *ngFor="let p of projects" [value]="p.id">{{ p.projectName }} ({{ p.projectCode }})</mat-option>
                </mat-select>
                <mat-error>Please select a project</mat-error>
              </mat-form-field>

              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Contract Amount *</mat-label>
                  <input matInput type="number" formControlName="contractAmount" placeholder="0">
                  <mat-icon matPrefix>currency_rupee</mat-icon>
                  <mat-error>Required</mat-error>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Advance Payment</mat-label>
                  <input matInput type="number" formControlName="advancePayment" placeholder="0">
                  <mat-icon matPrefix>payments</mat-icon>
                </mat-form-field>
              </div>

              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Currency</mat-label>
                  <mat-select formControlName="currency">
                    <mat-option value="INR">INR - Indian Rupee (₹)</mat-option>
                    <mat-option value="NPR">NPR - Nepali Rupee (रू)</mat-option>
                    <mat-option value="USD">USD - US Dollar ($)</mat-option>
                    <mat-option value="EUR">EUR - Euro (€)</mat-option>
                    <mat-option value="GBP">GBP - British Pound (£)</mat-option>
                  </mat-select>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Payment Terms</mat-label>
                  <input matInput formControlName="paymentTerms" placeholder="e.g., Net 30">
                </mat-form-field>
              </div>

              <mat-form-field appearance="outline">
                <mat-label>Payment Milestones</mat-label>
                <input matInput type="number" formControlName="paymentMilestones" placeholder="Number of milestones">
              </mat-form-field>
            </mat-card-content>
          </mat-card>

          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>account_balance_wallet</mat-icon>
              <mat-card-title>Bank Details</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Bank Name</mat-label>
                  <input matInput formControlName="bankName" placeholder="Enter bank name">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Account Number</mat-label>
                  <input matInput formControlName="bankAccountNumber" placeholder="Enter account number">
                </mat-form-field>
              </div>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Branch</mat-label>
                  <input matInput formControlName="bankBranch" placeholder="Enter branch">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>SWIFT Code</mat-label>
                  <input matInput formControlName="swiftCode" placeholder="Enter SWIFT code">
                </mat-form-field>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>event</mat-icon>
              <mat-card-title>Contract Signing</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <mat-form-field appearance="outline">
                <mat-label>Contract Signing Date *</mat-label>
                <input matInput [matDatepicker]="picker1" formControlName="contractSigningDate">
                <mat-datepicker-toggle matSuffix [for]="picker1"></mat-datepicker-toggle>
                <mat-datepicker #picker1></mat-datepicker>
                <mat-error>Required</mat-error>
              </mat-form-field>
            </mat-card-content>
          </mat-card>

          <div class="form-actions">
            <button mat-stroked-button type="button" routerLink="/financials">Cancel</button>
            <button mat-flat-button color="primary" type="submit" [disabled]="loading || form.invalid">
              <mat-icon>{{ isEdit ? 'save' : 'add' }}</mat-icon>
              {{ isEdit ? 'Update' : 'Create' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .form-page { max-width: 900px; margin: 0 auto; padding: 24px; }
    .form-header { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
    .form-header h1 { margin: 0; font-size: 1.5rem; font-weight: 700; }
    .form-header p { margin: 2px 0 0; color: #666; font-size: 0.9rem; }
    .form-body { display: flex; flex-direction: column; gap: 20px; }
    .form-card { border-radius: 12px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); border: 1px solid #eee; }
    .form-card mat-card-header { padding: 16px 20px 0; }
    .form-card mat-card-title { font-size: 1rem; font-weight: 600; }
    .form-card mat-card-content { padding: 16px 20px 20px; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    mat-form-field { width: 100%; }
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; padding: 8px 0 24px; }
    .form-actions button { min-width: 120px; height: 44px; font-weight: 500; }
    @media (max-width: 600px) { .grid-2 { grid-template-columns: 1fr; } .form-page { padding: 16px; } }
  `]
})
export class FinancialFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private srv = inject(FinancialService);
  private pSrv = inject(ProjectService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  projects: any[] = [];
  isEdit = false;
  id: string | null = null;
  loading = false;

  form = this.fb.group({
    projectId: ['', Validators.required],
    contractAmount: [0, [Validators.required, Validators.min(0)]],
    advancePayment: [0],
    currency: ['NPR'],
    paymentTerms: [''],
    paymentMilestones: [0],
    bankName: [''],
    bankAccountNumber: [''],
    bankBranch: [''],
    swiftCode: [''],
    contractSigningDate: [null as Date | null, Validators.required],
  });

  ngOnInit(): void {
    this.pSrv.getProjects({ page: 1, pageSize: 100 }).subscribe(r => {
      if (r.success) this.projects = r.data;
    });

    const iid = this.route.snapshot.paramMap.get('id');
    if (iid) { this.isEdit = true; this.id = iid; this.load(iid); }
  }

  load(id: string): void {
    this.loading = true;
    this.srv.getById(id).subscribe({
      next: (r) => {
        if (r?.data) {
          const d = r.data;
          this.form.patchValue({
            projectId: d.projectId, contractAmount: d.contractAmount, advancePayment: d.advancePayment || 0,
            currency: d.currency || 'NPR', paymentTerms: d.paymentTerms || '', paymentMilestones: d.paymentMilestones || 0,
            bankName: d.bankName || '', bankAccountNumber: d.bankAccountNumber || '', bankBranch: d.bankBranch || '',
            swiftCode: d.swiftCode || '',
            contractSigningDate: d.contractSigningDate ? moment(d.contractSigningDate).toDate() : null,
          });
        }
        this.loading = false;
      },
      error: () => { this.notify.error('Failed to load'); this.router.navigate(['/financials']); this.loading = false; }
    });
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const v = this.form.getRawValue();

    const body: any = {
      projectId: v.projectId,
      contractAmount: Number(v.contractAmount) || 0,
      advancePayment: Number(v.advancePayment) || 0,
      currency: v.currency || 'NPR',
      paymentTerms: (v.paymentTerms || '').trim(),
      paymentMilestones: Number(v.paymentMilestones) || 0,
      bankName: (v.bankName || '').trim(),
      bankAccountNumber: (v.bankAccountNumber || '').trim(),
      bankBranch: (v.bankBranch || '').trim(),
      swiftCode: (v.swiftCode || '').trim(),
      contractSigningDate: v.contractSigningDate ? moment(v.contractSigningDate).toISOString() : new Date().toISOString(),
    };

    const req$ = this.isEdit ? this.srv.update(this.id!, body) : this.srv.create(body);
    req$.subscribe({
      next: (r) => {
        this.loading = false;
        if (r.success) { this.notify.success(this.isEdit ? 'Updated' : 'Created'); this.router.navigate(['/financials']); }
        else this.notify.error(r.message || 'Failed');
      },
      error: (e) => { this.loading = false; this.notify.error(e?.error?.message || 'Failed'); }
    });
  }
}