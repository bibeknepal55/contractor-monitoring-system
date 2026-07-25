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
import { ProjectService } from '../../../core/services/project.service';
import { ContractorService } from '../../../core/services/contractor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Contractor } from '../../../core/models/contractor.model';
import moment from 'moment';

@Component({
  selector: 'app-project-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink,
  ],
  template: `
    <div class="form-page">
      <div class="form-header">
        <button mat-icon-button routerLink="/projects" class="back-btn"><mat-icon>arrow_back</mat-icon></button>
        <div><h1>{{ isEdit ? 'Edit Project' : 'New Project' }}</h1><p>{{ isEdit ? 'Update project information' : 'Create a new infrastructure project' }}</p></div>
      </div>
      <div class="form-body">
        <form [formGroup]="form" (ngSubmit)="save()">
          <mat-card class="form-card"><mat-card-header><mat-icon mat-card-avatar>info</mat-icon><mat-card-title>Basic Details</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline"><mat-label>Project Code</mat-label><input matInput formControlName="projectCode" placeholder="PRJ-001"><mat-error>Required</mat-error></mat-form-field>
                <mat-form-field appearance="outline"><mat-label>Project Name</mat-label><input matInput formControlName="projectName" placeholder="Enter name"><mat-error>Required</mat-error></mat-form-field>
              </div>
              <div class="grid-2">
                <mat-form-field appearance="outline"><mat-label>Status</mat-label><mat-select formControlName="status"><mat-option value="Planned">Planned</mat-option><mat-option value="Active">Active</mat-option><mat-option value="OnHold">On Hold</mat-option><mat-option value="Completed">Completed</mat-option><mat-option value="Cancelled">Cancelled</mat-option><mat-option value="Delayed">Delayed</mat-option></mat-select></mat-form-field>
                <mat-form-field appearance="outline"><mat-label>Priority</mat-label><mat-select formControlName="priority"><mat-option value="Low">Low</mat-option><mat-option value="Medium">Medium</mat-option><mat-option value="High">High</mat-option><mat-option value="Critical">Critical</mat-option></mat-select></mat-form-field>
              </div>
              <mat-form-field appearance="outline"><mat-label>Description</mat-label><textarea matInput formControlName="description" rows="2" placeholder="Brief description"></textarea></mat-form-field>
            </mat-card-content></mat-card>
          <mat-card class="form-card"><mat-card-header><mat-icon mat-card-avatar>location_city</mat-icon><mat-card-title>Location & Financials</mat-card-title></mat-card-header>
            <mat-card-content><div class="grid-2">
              <mat-form-field appearance="outline"><mat-label>Location</mat-label><input matInput formControlName="location" placeholder="City, State"></mat-form-field>
              <mat-form-field appearance="outline"><mat-label>Budget (₹)</mat-label><input matInput type="number" formControlName="budget" placeholder="0"><mat-error>Required</mat-error></mat-form-field>
            </div></mat-card-content></mat-card>
          <mat-card class="form-card"><mat-card-header><mat-icon mat-card-avatar>people</mat-icon><mat-card-title>Management</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline"><mat-label>Project Manager</mat-label><input matInput formControlName="projectManager" placeholder="Full name"></mat-form-field>
                <mat-form-field appearance="outline"><mat-label>Contact Number</mat-label><input matInput formControlName="contactNumber" placeholder="+91-"></mat-form-field>
              </div>
              <div class="grid-2">
                <mat-form-field appearance="outline"><mat-label>Contract Number</mat-label><input matInput formControlName="contractNumber" placeholder="CN-"></mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Contractor *</mat-label>
                  <mat-select formControlName="contractorId" placeholder="Select contractor">
                    <mat-option *ngFor="let c of contractors" [value]="c.id">{{c.companyName}}</mat-option>
                  </mat-select>
                  <mat-error>Required</mat-error>
                </mat-form-field>
              </div>
            </mat-card-content></mat-card>
          <mat-card class="form-card"><mat-card-header><mat-icon mat-card-avatar>event</mat-icon><mat-card-title>Timeline</mat-card-title></mat-card-header>
            <mat-card-content><div class="grid-2">
              <mat-form-field appearance="outline"><mat-label>Start Date</mat-label><input matInput [matDatepicker]="p1" formControlName="startDate"><mat-datepicker-toggle matSuffix [for]="p1"></mat-datepicker-toggle><mat-datepicker #p1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
              <mat-form-field appearance="outline"><mat-label>End Date</mat-label><input matInput [matDatepicker]="p2" formControlName="endDate"><mat-datepicker-toggle matSuffix [for]="p2"></mat-datepicker-toggle><mat-datepicker #p2></mat-datepicker></mat-form-field>
            </div></mat-card-content></mat-card>
          <div class="form-actions">
            <button mat-stroked-button type="button" routerLink="/projects">Cancel</button>
            <button mat-flat-button color="primary" type="submit" [disabled]="loading || form.invalid"><mat-icon>{{ isEdit ? 'save' : 'add' }}</mat-icon>{{ isEdit ? 'Update' : 'Create' }}</button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.form-header{display:flex;align-items:center;gap:16px;margin-bottom:24px}.form-header h1{margin:0;font-size:1.5rem;font-weight:700}.form-header p{margin:2px 0 0;color:#666}.form-body{display:flex;flex-direction:column;gap:20px}.form-card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);border:1px solid #eee}.form-card mat-card-header{padding:16px 20px 0}.form-card mat-card-title{font-size:1rem;font-weight:600}.form-card mat-card-content{padding:16px 20px 20px}.grid-2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.form-actions{display:flex;justify-content:flex-end;gap:12px;padding:8px 0 24px}.form-actions button{min-width:120px;height:44px;font-weight:500}@media(max-width:600px){.grid-2{grid-template-columns:1fr}.form-page{padding:16px}}`]
})
export class ProjectFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private service = inject(ProjectService);
  private contractorService = inject(ContractorService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  contractors: Contractor[] = [];

  form = this.fb.group({
    projectCode: ['', Validators.required],
    projectName: ['', Validators.required],
    status: ['Planned'], priority: ['Medium'], location: [''],
    budget: [0, [Validators.required, Validators.min(0)]],
    projectManager: [''], contactNumber: [''], contractNumber: [''],
    contractorId: ['', Validators.required],
    startDate: [null as Date | null, Validators.required],
    endDate: [null as Date | null], description: [''],
  });

  isEdit = false;
  projectId: string | null = null;
  loading = false;

  ngOnInit(): void {
    this.loadContractors();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) { this.isEdit = true; this.projectId = id; this.loadProject(id); }
  }

  loadContractors(): void {
    this.contractorService.getContractors({ page: 1, pageSize: 100 }).subscribe({
      next: (r) => { if (r.success) this.contractors = r.data; }
    });
  }

  loadProject(id: string): void {
    this.loading = true;
    this.service.getProjectById(id).subscribe({
      next: (r) => {
        if (r?.data) {
          const p = r.data;
          this.form.patchValue({
            projectCode: p.projectCode, projectName: p.projectName, status: p.status, priority: p.priority || 'Medium',
            location: p.location, budget: p.budget, projectManager: p.projectManager, contactNumber: p.contactNumber,
            contractNumber: p.contractNumber, contractorId: p.contractorId,
            startDate: p.startDate ? moment(p.startDate).toDate() : null,
            endDate: p.endDate ? moment(p.endDate).toDate() : null, description: p.description,
          });
        }
        this.loading = false;
      },
      error: () => { this.notify.error('Failed to load'); this.router.navigate(['/projects']); this.loading = false; }
    });
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const v = this.form.getRawValue();

    // Send directly without wrapper - NO { request: {} }
    const body: any = {
      projectCode: (v.projectCode || '').trim(),
      projectName: (v.projectName || '').trim(),
      status: v.status || 'Planned',
      priority: v.priority || 'Medium',
      location: (v.location || '').trim(),
      budget: Number(v.budget) || 0,
      projectManager: (v.projectManager || '').trim(),
      contactNumber: (v.contactNumber || '').trim(),
      contractNumber: (v.contractNumber || '').trim(),
      description: (v.description || '').trim(),
      startDate: v.startDate ? moment(v.startDate).toISOString() : new Date().toISOString(),
      contractorId: v.contractorId,
    };
    if (v.endDate) { body.endDate = moment(v.endDate).toISOString(); }

    const req$ = this.isEdit ? this.service.updateProject(this.projectId!, body) : this.service.createProject(body);
    req$.subscribe({
      next: (r) => {
        this.loading = false;
        if (r.success) { this.notify.success(this.isEdit ? 'Updated' : 'Created'); this.router.navigate(['/projects']); }
        else this.notify.error(r.message || 'Failed');
      },
      error: (e) => {
        this.loading = false;
        const errs = e?.error?.errors;
        const msg = errs ? Object.values(errs).flat().join('. ') : e?.error?.message || 'Failed';
        this.notify.error(msg);
      }
    });
  }
}