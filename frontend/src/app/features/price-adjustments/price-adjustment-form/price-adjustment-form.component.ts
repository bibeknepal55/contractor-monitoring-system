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
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
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
    MatSlideToggleModule, MatTooltipModule, MatChipsModule, RouterLink,
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
                <!-- Reference Document Name -->
                <mat-form-field appearance="outline">
                  <mat-label>Reference Document Name</mat-label>
                  <input matInput formControlName="referenceDocument" placeholder="e.g., REF-2026-001">
                  <mat-icon matPrefix>description</mat-icon>
                </mat-form-field>
                
                <!-- File Upload -->
                <div class="file-upload-wrapper">
                  <label class="file-label">Attach Document</label>
                  <div class="file-upload-area" (click)="fileInput.click()" 
                    [class.has-file]="selectedFile || form.get('attachmentUrl')?.value">
                    <input #fileInput type="file" (change)="onFileSelected($event)" 
                      accept=".pdf,.doc,.docx,.xls,.xlsx,.jpg,.png,.jpeg" style="display:none;">
                    <mat-icon class="upload-icon">
                      {{ selectedFile || form.get('attachmentUrl')?.value ? 'description' : 'cloud_upload' }}
                    </mat-icon>
                    <div class="upload-text">
                      <span class="upload-title">
                        {{ selectedFile ? selectedFile.name : (form.get('attachmentUrl')?.value ? 'Document attached' : 'Click to upload') }}
                      </span>
                      <span class="upload-hint" *ngIf="!selectedFile && !form.get('attachmentUrl')?.value">
                        PDF, DOC, XLS, JPG, PNG (Max 10MB)
                      </span>
                      <span class="upload-hint" *ngIf="selectedFile">
                        {{ (selectedFile.size / 1024 / 1024).toFixed(2) }} MB
                      </span>
                    </div>
                    <button mat-icon-button type="button" class="remove-file-btn" 
                      *ngIf="selectedFile || form.get('attachmentUrl')?.value"
                      (click)="removeFile($event)" matTooltip="Remove file">
                      <mat-icon>close</mat-icon>
                    </button>
                  </div>
                </div>
              </div>

              <div class="grid-2">
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

          <!-- Approval - Only visible to users with full CRUD permissions -->
          <mat-card class="form-card" *ngIf="canApprove">
            <mat-card-header>
              <mat-icon mat-card-avatar>fact_check</mat-icon>
              <mat-card-title>Approval Status</mat-card-title>
              <mat-card-subtitle>Only visible to authorized approvers</mat-card-subtitle>
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
    .form-card mat-card-subtitle { font-size: 0.75rem; color: #999; }
    .form-card mat-card-content { padding: 16px 20px 20px; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .grid-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 16px; }
    mat-form-field { width: 100%; }
    .impact-summary { display: flex; align-items: center; gap: 8px; padding: 12px 16px; background: #f8f9fa; border-radius: 8px; margin-top: 8px; }
    
    /* File Upload */
    .file-upload-wrapper { display: flex; flex-direction: column; gap: 4px; }
    .file-label { font-size: 0.75rem; color: rgba(0,0,0,0.6); font-weight: 500; }
    .file-upload-area { 
      display: flex; align-items: center; gap: 12px; padding: 12px 16px;
      border: 2px dashed #d0d5dd; border-radius: 8px; cursor: pointer;
      transition: all 0.2s; min-height: 48px; position: relative;
    }
    .file-upload-area:hover { border-color: #1a73e8; background: #f8faff; }
    .file-upload-area.has-file { border-color: #059669; border-style: solid; background: #f9fefb; }
    .upload-icon { color: #1a73e8; font-size: 28px; width: 28px; height: 28px; flex-shrink: 0; }
    .has-file .upload-icon { color: #059669; }
    .upload-text { display: flex; flex-direction: column; gap: 2px; flex: 1; min-width: 0; }
    .upload-title { font-size: 0.85rem; color: #333; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .upload-hint { font-size: 0.72rem; color: #999; }
    .remove-file-btn { position: absolute; top: 4px; right: 4px; color: #dc2626; }

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
  selectedFile: File | null = null;

  // Only users with Create+Update permission (or SuperAdmin/Admin) can approve
  get canApprove(): boolean {
    const hasCreate = this.auth.hasPermission('PriceAdjustment.Create');
    const hasUpdate = this.auth.hasPermission('PriceAdjustment.Update');
    const hasView = this.auth.hasPermission('PriceAdjustment.View');
    const isAdmin = this.auth.hasAnyRole(['SuperAdmin', 'Admin']);
    // User must have Create AND Update AND View to see approval
    return isAdmin || (hasCreate && hasUpdate && hasView);
  }

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
    attachmentUrl: [''],
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

    this.form.get('previousAmount')?.valueChanges.subscribe(() => this.calculatePercentage());
    this.form.get('newAmount')?.valueChanges.subscribe(() => this.calculatePercentage());

    const iid = this.route.snapshot.paramMap.get('id');
    if (iid) { this.isEdit = true; this.id = iid; this.load(iid); }
  }

  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (!file) return;
    
    // Validate file size (10MB max)
    if (file.size > 10 * 1024 * 1024) {
      this.notify.error('File size must be less than 10MB');
      return;
    }
    
    // Validate file type
    const allowedTypes = ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.jpg', '.jpeg', '.png'];
    const ext = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!allowedTypes.includes(ext)) {
      this.notify.error('Invalid file type. Allowed: PDF, DOC, XLS, JPG, PNG');
      return;
    }
    
    this.selectedFile = file;
    // Auto-fill reference name from file if empty
    if (!this.form.get('referenceDocument')?.value) {
      this.form.patchValue({ referenceDocument: file.name.replace(ext, '') });
    }
    event.target.value = '';
  }

  removeFile(event: Event): void {
    event.stopPropagation();
    this.selectedFile = null;
    this.form.patchValue({ attachmentUrl: '' });
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
            attachmentUrl: d.attachmentUrl || '',
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

    // If user can't approve, force isApproved to false
    const isApproved = this.canApprove ? (v.isApproved || false) : false;
    const effectiveDate = this.canApprove ? (v.effectiveDate ? moment(v.effectiveDate).toISOString() : null) : null;

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
      attachmentUrl: v.attachmentUrl || '',
      requestedBy: (v.requestedBy || '').trim() || this.currentUser,
      remarks: (v.remarks || '').trim(),
      isApproved: isApproved,
      effectiveDate: effectiveDate,
    };

    // Upload file first if selected
    if (this.selectedFile) {
      this.uploadFileAndSave(body);
    } else {
      this.submitData(body);
    }
  }

  private uploadFileAndSave(body: any): void {
    if (!this.selectedFile) { this.submitData(body); return; }
    
    // Convert file to base64 for storage (or use multipart upload)
    const reader = new FileReader();
    reader.onload = (e: any) => {
      body.attachmentUrl = e.target.result;
      body.attachmentFileName = this.selectedFile!.name;
      body.attachmentFileSize = this.selectedFile!.size;
      this.submitData(body);
    };
    reader.readAsDataURL(this.selectedFile);
  }

  private submitData(body: any): void {
    const req$ = this.isEdit ? this.srv.update(this.id!, body) : this.srv.create(body);
    req$.subscribe({
      next: (r) => {
        this.loading = false;
        if (r.success) {
          const action = this.isEdit ? 'updated' : 'created';
          const approvalMsg = body.isApproved ? ' and approved' : '';
          this.notify.success(`Price adjustment ${action}${approvalMsg}!`);
          this.router.navigate(['/price-adjustments']);
        } else {
          this.notify.error(r.message || 'Failed');
        }
      },
      error: (e) => {
        this.loading = false;
        this.notify.error(e?.error?.message || 'Failed');
      }
    });
  }
}