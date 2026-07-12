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
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PriceAdjustmentService } from '../../../core/services/price-adjustment.service';
import { ProjectService } from '../../../core/services/project.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-price-adjustment-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule,
    MatSlideToggleModule, RouterLink,
  ],
  template: `
    <div class="form-page">
      <div class="form-header">
        <button mat-icon-button routerLink="/price-adjustments" class="back-btn">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <div>
          <h1>{{ isEdit ? 'Edit Price Adjustment' : 'New Price Adjustment' }}</h1>
          <p>Document budget changes with full audit trail</p>
        </div>
      </div>

      <div class="form-body">
        <form [formGroup]="form" (ngSubmit)="save()">
          <!-- Project & Type -->
          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>business</mat-icon>
              <mat-card-title>Project & Category</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Project *</mat-label>
                  <mat-select formControlName="projectId">
                    <mat-option *ngFor="let p of projects" [value]="p.id">
                      {{ p.projectName }} ({{ p.projectCode }})
                    </mat-option>
                  </mat-select>
                  <mat-error>Select a project</mat-error>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Adjustment Type *</mat-label>
                  <mat-select formControlName="adjustmentType">
                    <mat-option value="Escalation">📈 Escalation</mat-option>
                    <mat-option value="Material">🧱 Material Cost Change</mat-option>
                    <mat-option value="Labor">👷 Labor Cost Change</mat-option>
                    <mat-option value="Scope Change">📋 Scope Change</mat-option>
                    <mat-option value="Regulatory">⚖️ Regulatory Requirement</mat-option>
                    <mat-option value="Other">📌 Other</mat-option>
                  </mat-select>
                  <mat-error>Select type</mat-error>
                </mat-form-field>
              </div>
            </mat-card-content>
          </mat-card>

          <!-- Financial Impact -->
          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>trending_up</mat-icon>
              <mat-card-title>Financial Impact</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-3">
                <mat-form-field appearance="outline">
                  <mat-label>Previous Amount</mat-label>
                  <input matInput type="number" formControlName="previousAmount" placeholder="0">
                  <mat-icon matPrefix>arrow_back</mat-icon>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>New Amount *</mat-label>
                  <input matInput type="number" formControlName="newAmount" placeholder="0">
                  <mat-icon matPrefix>arrow_forward</mat-icon>
                  <mat-error>Required</mat-error>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>% Change</mat-label>
                  <input matInput type="number" formControlName="percentageChange" placeholder="Auto" readonly>
                  <mat-icon matPrefix>percent</mat-icon>
                </mat-form-field>
              </div>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Currency</mat-label>
                  <mat-select formControlName="currency">
                    <mat-option value="NPR">NPR - Nepali Rupee (रू)</mat-option>
                    <mat-option value="INR">INR - Indian Rupee (₹)</mat-option>
                    <mat-option value="USD">USD - US Dollar ($)</mat-option>
                  </mat-select>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Adjustment Date</mat-label>
                  <input matInput [matDatepicker]="picker1" formControlName="adjustmentDate">
                  <mat-datepicker-toggle matSuffix [for]="picker1"></mat-datepicker-toggle>
                  <mat-datepicker #picker1></mat-datepicker>
                </mat-form-field>
              </div>
              <div class="impact-summary" *ngIf="form.get('previousAmount')?.value && form.get('newAmount')?.value">
                <mat-icon [style.color]="getNetChange() >= 0 ? '#137333' : '#c5221f'">
                  {{ getNetChange() >= 0 ? 'trending_up' : 'trending_down' }}
                </mat-icon>
                <span [style.color]="getNetChange() >= 0 ? '#137333' : '#c5221f'" style="font-weight:600;">
                  Net Impact: {{ getNetChange() >= 0 ? '+' : '' }}रू {{ getNetChange() | number:'1.0-0' }}
                  ({{ getPercentageChange() }}%)
                </span>
              </div>
            </mat-card-content>
          </mat-card>

          <!-- Justification & References -->
          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>description</mat-icon>
              <mat-card-title>Justification & References</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <mat-form-field appearance="outline">
                <mat-label>Reason / Justification *</mat-label>
                <textarea matInput formControlName="reason" rows="3" placeholder="Detailed reason for this price adjustment"></textarea>
                <mat-error>Reason is required</mat-error>
              </mat-form-field>

              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Reference Document</mat-label>
                  <input matInput formControlName="referenceDocument" placeholder="e.g., REF-2026-001">
                  <mat-icon matPrefix>attachment</mat-icon>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Requested By</mat-label>
                  <input matInput formControlName="requestedBy">
                  <mat-icon matPrefix>person</mat-icon>
                </mat-form-field>
              </div>

              <mat-form-field appearance="outline">
                <mat-label>Remarks</mat-label>
                <textarea matInput formControlName="remarks" rows="2" placeholder="Any additional notes"></textarea>
                <mat-icon matPrefix>notes</mat-icon>
              </mat-form-field>
            </mat-card-content>
          </mat-card>

          <!-- Approval -->
          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>fact_check</mat-icon>
              <mat-card-title>Approval Status</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-slide-toggle formControlName="isApproved" color="primary">
                  {{ form.get('isApproved')?.value ? 'Approved' : 'Pending Approval' }}
                </mat-slide-toggle>
                <mat-form-field appearance="outline" *ngIf="form.get('isApproved')?.value">
                  <mat-label>Effective Date</mat-label>
                  <input matInput [matDatepicker]="picker2" formControlName="effectiveDate">
                  <mat-datepicker-toggle matSuffix [for]="picker2"></mat-datepicker-toggle>
                  <mat-datepicker #picker2></mat-datepicker>
                </mat-form-field>
              </div>
            </mat-card-content>
          </mat-card>

          <div class="form-actions">
            <button mat-stroked-button type="button" routerLink="/price-adjustments">Cancel</button>
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
    .form-page { max-width: 960px; margin: 0 auto; padding: 24px; animation: fadeIn 0.3s; }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
    .form-header { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
    .form-header h1 { margin: 0; font-size: 1.5rem; font-weight: 700; color: #1a1a1a; }
    .form-header p { margin: 2px 0 0; color: #666; font-size: 0.9rem; }
    .form-body { display: flex; flex-direction: column; gap: 20px; }
    .form-card { border-radius: 12px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); border: 1px solid #e8eaed; }
    .form-card mat-card-header { padding: 16px 20px 0; }
    .form-card mat-card-title { font-size: 1rem; font-weight: 600; color: #333; }
    .form-card mat-card-content { padding: 16px 20px 20px; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .grid-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 16px; }
    mat-form-field { width: 100%; }
    .impact-summary { display: flex; align-items: center; gap: 8px; padding: 12px 16px; background: #f8f9fa; border-radius: 8px; margin-top: 8px; }
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; padding: 8px 0 24px; }
    .form-actions button { min-width: 130px; height: 44px; font-weight: 500; border-radius: 8px; }
    @media (max-width: 768px) { .grid-2, .grid-3 { grid-template-columns: 1fr; } .form-page { padding: 16px; } }
  `]
})
export class PriceAdjustmentFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private srv = inject(PriceAdjustmentService);
  private pSrv = inject(ProjectService);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  projects: any[] = [];
  currentUser = '';
  isEdit = false;
  id: string | null = null;
  loading = false;

  form = this.fb.group({
    projectId: ['', Validators.required],
    adjustmentType: ['', Validators.required],
    previousAmount: [0],
    newAmount: [0, [Validators.required, Validators.min(0)]],
    percentageChange: [{ value: 0, disabled: true }],
    currency: ['NPR'],
    adjustmentDate: [new Date()],
    reason: ['', Validators.required],
    referenceDocument: [''],
    requestedBy: [''],
    remarks: [''],
    isApproved: [false],
    effectiveDate: [null as Date | null],
  });

  ngOnInit(): void {
    const user = this.auth.getCurrentUser();
    this.currentUser = user ? `${user.firstName} ${user.lastName}` : 'Project Manager';
    this.form.patchValue({ requestedBy: this.currentUser, adjustmentDate: new Date() });

    this.pSrv.getProjects({ page: 1, pageSize: 100 }).subscribe(r => {
      if (r.success) this.projects = r.data;
    });

    // Auto-calculate percentage change
    this.form.get('previousAmount')?.valueChanges.subscribe(() => this.calculatePercentage());
    this.form.get('newAmount')?.valueChanges.subscribe(() => this.calculatePercentage());

    const iid = this.route.snapshot.paramMap.get('id');
    if (iid) { this.isEdit = true; this.id = iid; this.load(iid); }
  }

  calculatePercentage(): void {
    const prev = Number(this.form.get('previousAmount')?.value) || 0;
    const curr = Number(this.form.get('newAmount')?.value) || 0;
    if (prev > 0) {
      const pct = ((curr - prev) / prev) * 100;
      this.form.patchValue({ percentageChange: Math.round(pct * 100) / 100 }, { emitEvent: false });
    }
  }

  getNetChange(): number {
    const prev = Number(this.form.get('previousAmount')?.value) || 0;
    const curr = Number(this.form.get('newAmount')?.value) || 0;
    return curr - prev;
  }

  getPercentageChange(): number {
    const prev = Number(this.form.get('previousAmount')?.value) || 0;
    if (prev === 0) return 0;
    return Math.round(((Number(this.form.get('newAmount')?.value) || 0) - prev) / prev * 100 * 100) / 100;
  }

  load(id: string): void {
    this.loading = true;
    this.srv.getById(id).subscribe({
      next: (r) => {
        if (r?.data) {
          const d = r.data;
          this.form.patchValue({
            projectId: d.projectId,
            adjustmentType: d.adjustmentType,
            previousAmount: d.previousAmount || 0,
            newAmount: d.newAmount || d.amount || 0,
            percentageChange: d.percentageChange || 0,
            currency: d.currency || 'NPR',
            adjustmentDate: d.adjustmentDate ? moment(d.adjustmentDate).toDate() : new Date(),
            reason: d.reason,
            referenceDocument: d.referenceDocument || '',
            requestedBy: d.requestedBy || this.currentUser,
            remarks: d.remarks || '',
            isApproved: d.isApproved || false,
            effectiveDate: d.effectiveDate ? moment(d.effectiveDate).toDate() : null,
          });
        }
        this.loading = false;
      },
      error: () => { this.notify.error('Failed to load'); this.router.navigate(['/price-adjustments']); this.loading = false; }
    });
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const v = this.form.getRawValue();

    const body: any = {
      projectId: v.projectId,
      adjustmentType: v.adjustmentType,
      previousAmount: Number(v.previousAmount) || 0,
      newAmount: Number(v.newAmount) || 0,
      percentageChange: Number(v.percentageChange) || 0,
      currency: v.currency || 'NPR',
      adjustmentDate: v.adjustmentDate ? moment(v.adjustmentDate).toISOString() : new Date().toISOString(),
      reason: (v.reason || '').trim(),
      referenceDocument: (v.referenceDocument || '').trim(),
      requestedBy: (v.requestedBy || '').trim() || this.currentUser,
      remarks: (v.remarks || '').trim(),
      isApproved: v.isApproved || false,
      effectiveDate: v.effectiveDate ? moment(v.effectiveDate).toISOString() : null,
    };

    console.log('Saving adjustment:', this.isEdit ? 'UPDATE' : 'CREATE', body);

    const req$ = this.isEdit ? this.srv.update(this.id!, body) : this.srv.create(body);
    req$.subscribe({
      next: (r) => {
        this.loading = false;
        console.log('Save response:', r);
        if (r.success) {
          this.notify.success(this.isEdit ? 'Price adjustment updated!' : 'Price adjustment created!');
          this.router.navigate(['/price-adjustments']);
        } else {
          this.notify.error(r.message || 'Failed');
        }
      },
      error: (e) => {
        this.loading = false;
        console.error('Save error:', e);
        this.notify.error(e?.error?.message || 'Failed');
      }
    });
  }
}