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
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ContractorService } from '../../../core/services/contractor.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-contractor-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule,
    MatSnackBarModule, RouterLink,
  ],
  template: `
    <div class="form-page">
      <div class="form-header">
        <button mat-icon-button routerLink="/contractors" class="back-btn">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <div>
          <h1>{{ isEdit ? 'Edit Contractor' : 'New Contractor' }}</h1>
          <p>{{ isEdit ? 'Update contractor details' : 'Register a new contractor' }}</p>
        </div>
      </div>

      <div class="form-body">
        <form [formGroup]="form" (ngSubmit)="save()">
          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>business</mat-icon>
              <mat-card-title>Company Information</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Company Name</mat-label>
                  <input matInput formControlName="companyName" placeholder="Enter company name">
                  <mat-error>Required</mat-error>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Status</mat-label>
                  <mat-select formControlName="status">
                    <mat-option value="Active">Active</mat-option>
                    <mat-option value="Inactive">Inactive</mat-option>
                    <mat-option value="Blacklisted">Blacklisted</mat-option>
                    <mat-option value="UnderReview">Under Review</mat-option>
                  </mat-select>
                </mat-form-field>
              </div>
              <div class="grid-3">
                <mat-form-field appearance="outline">
                  <mat-label>Registration Number</mat-label>
                  <input matInput formControlName="registrationNumber">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Tax ID</mat-label>
                  <input matInput formControlName="taxId">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>License Number</mat-label>
                  <input matInput formControlName="licenseNumber">
                </mat-form-field>
              </div>
              <mat-form-field appearance="outline">
                <mat-label>Address</mat-label>
                <input matInput formControlName="address" placeholder="Street address">
              </mat-form-field>
              <div class="grid-3">
                <mat-form-field appearance="outline">
                  <mat-label>City</mat-label>
                  <input matInput formControlName="city">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>State</mat-label>
                  <input matInput formControlName="state">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Postal Code</mat-label>
                  <input matInput formControlName="postalCode">
                </mat-form-field>
              </div>
              <mat-form-field appearance="outline">
                <mat-label>Country</mat-label>
                <input matInput formControlName="country">
              </mat-form-field>
            </mat-card-content>
          </mat-card>

          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>contact_phone</mat-icon>
              <mat-card-title>Contact Details</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Phone</mat-label>
                  <input matInput formControlName="phone" placeholder="+91-">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Email</mat-label>
                  <input matInput formControlName="email" type="email" placeholder="company@mail.com">
                </mat-form-field>
              </div>
              <mat-form-field appearance="outline">
                <mat-label>Website</mat-label>
                <input matInput formControlName="website" placeholder="https://">
              </mat-form-field>
            </mat-card-content>
          </mat-card>

          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>person</mat-icon>
              <mat-card-title>Contact Person</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>Name</mat-label>
                  <input matInput formControlName="contactPerson">
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Phone</mat-label>
                  <input matInput formControlName="contactPersonPhone">
                </mat-form-field>
              </div>
              <mat-form-field appearance="outline">
                <mat-label>Email</mat-label>
                <input matInput formControlName="contactPersonEmail" type="email">
              </mat-form-field>
            </mat-card-content>
          </mat-card>

          <mat-card class="form-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>verified</mat-icon>
              <mat-card-title>License & Insurance</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="grid-2">
                <mat-form-field appearance="outline">
                  <mat-label>License Expiry Date</mat-label>
                  <input matInput [matDatepicker]="picker3" formControlName="licenseExpiryDate">
                  <mat-datepicker-toggle matSuffix [for]="picker3"></mat-datepicker-toggle>
                  <mat-datepicker #picker3></mat-datepicker>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Insurance Details</mat-label>
                  <input matInput formControlName="insuranceDetails">
                </mat-form-field>
              </div>
            </mat-card-content>
          </mat-card>

          <div class="form-actions">
            <button mat-stroked-button type="button" routerLink="/contractors">Cancel</button>
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
    .form-page { max-width: 900px; margin: 0 auto; padding: 24px; animation: fadeIn 0.3s; }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
    .form-header { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
    .back-btn { flex-shrink: 0; }
    .form-header h1 { margin: 0; font-size: 1.5rem; font-weight: 700; color: #1a1a1a; }
    .form-header p { margin: 2px 0 0; color: #666; font-size: 0.9rem; }
    .form-body { display: flex; flex-direction: column; gap: 20px; }
    .form-card { border-radius: 12px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); border: 1px solid #eee; }
    .form-card mat-card-header { padding: 16px 20px 0; }
    .form-card mat-card-title { font-size: 1rem; font-weight: 600; }
    .form-card mat-card-content { padding: 16px 20px 20px; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .grid-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 16px; }
    mat-form-field { width: 100%; }
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; padding: 8px 0 24px; }
    .form-actions button { min-width: 120px; height: 44px; font-weight: 500; }
    @media (max-width: 768px) { .grid-2, .grid-3 { grid-template-columns: 1fr; } .form-page { padding: 16px; } }
  `]
})
export class ContractorFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ContractorService);
  private readonly notify = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  form = this.fb.group({
    companyName: ['', Validators.required],
    status: ['Active'],
    registrationNumber: [''],
    taxId: [''],
    licenseNumber: [''],
    address: [''],
    city: [''],
    state: [''],
    postalCode: [''],
    country: [''],
    phone: [''],
    email: [''],
    website: [''],
    contactPerson: [''],
    contactPersonPhone: [''],
    contactPersonEmail: [''],
    licenseExpiryDate: [null as Date | null],
    insuranceDetails: [''],
  });

  isEdit = false;
  contractorId: string | null = null;
  loading = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) { this.isEdit = true; this.contractorId = id; this.load(id); }
  }

  load(id: string): void {
    this.loading = true;
    this.service.getContractorById(id).subscribe({
      next: (r) => {
        if (r?.data) {
          const c = r.data;
          this.form.patchValue({
            companyName: c.companyName, status: c.status || 'Active', registrationNumber: c.registrationNumber,
            taxId: c.taxId, licenseNumber: c.licenseNumber, address: c.address, city: c.city, state: c.state,
            postalCode: c.postalCode, country: c.country, phone: c.phone, email: c.email, website: c.website,
            contactPerson: c.contactPerson, contactPersonPhone: c.contactPersonPhone,
            contactPersonEmail: c.contactPersonEmail,
            licenseExpiryDate: c.licenseExpiryDate ? moment(c.licenseExpiryDate).toDate() : null,
            insuranceDetails: c.insuranceDetails,
          });
        }
        this.loading = false;
      },
      error: () => { this.notify.error('Failed to load'); this.router.navigate(['/contractors']); this.loading = false; }
    });
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const v = this.form.getRawValue();

    const body: Record<string, any> = {};
    Object.keys(v).forEach(k => {
      const val = (v as any)[k];
      if (val instanceof Date) body[k] = moment(val).toISOString();
      else if (typeof val === 'string') body[k] = val.trim();
      else body[k] = val;
    });

    const req$ = this.isEdit ? this.service.updateContractor(this.contractorId!, body) : this.service.createContractor(body);
    req$.subscribe({
      next: (r) => {
        this.loading = false;
        if (r.success) { this.notify.success(this.isEdit ? 'Contractor updated' : 'Contractor created'); this.router.navigate(['/contractors']); }
        else this.notify.error(r.message || 'Failed');
      },
      error: (e) => {
        this.loading = false;
        this.notify.error(e?.error?.message || 'Operation failed');
      }
    });
  }
}