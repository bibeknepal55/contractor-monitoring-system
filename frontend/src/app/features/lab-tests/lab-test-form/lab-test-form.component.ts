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
import { LabTestService } from '../../../core/services/lab-test.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-lab-test-form', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink],
  template: `
    <div class="form-page">
      <div class="top-bar"><button mat-icon-button routerLink="/lab-tests"><mat-icon>arrow_back</mat-icon></button><div><h1>{{edit?'Edit':'Record'}} Lab Test</h1><p>Document material quality testing results</p></div></div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Test Info</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Test Code *</mat-label><input matInput formControlName="testCode" placeholder="e.g., LT-CON-001"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Test Name *</mat-label><input matInput formControlName="testName" placeholder="e.g., Compressive Strength"><mat-error>Required</mat-error></mat-form-field>
          </div>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Category</mat-label><mat-select formControlName="category"><mat-option value="Concrete">Concrete</mat-option><mat-option value="Soil">Soil</mat-option><mat-option value="Steel">Steel</mat-option><mat-option value="Asphalt">Asphalt</mat-option><mat-option value="Water">Water</mat-option><mat-option value="Aggregate">Aggregate</mat-option><mat-option value="Cement">Cement</mat-option><mat-option value="Other">Other</mat-option></mat-select></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Test Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="testDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
          </div>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>biotech</mat-icon><mat-card-title>Lab & Personnel</mat-card-title></mat-card-header><mat-card-content>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Lab Name *</mat-label><input matInput formControlName="labName" placeholder="e.g., National Materials Lab"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Technician Name *</mat-label><input matInput formControlName="technicianName" placeholder="Technician full name"><mat-error>Required</mat-error></mat-form-field>
          </div>
          <mat-form-field appearance="outline"><mat-label>Test Standard</mat-label><input matInput formControlName="testStandard" placeholder="e.g., IS 456:2000, ASTM C39"></mat-form-field>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>checklist</mat-icon><mat-card-title>Parameters & Limits</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Parameter Tested *</mat-label><input matInput formControlName="parameterTested" placeholder="e.g., Compressive Strength at 28 days"><mat-error>Required</mat-error></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Specification Limit</mat-label><input matInput formControlName="specificationLimit" placeholder="e.g., Min 30 MPa, Max 5% variation"></mat-form-field>
        </mat-card-content></mat-card>

        <div class="btns"><button mat-stroked-button type="button" routerLink="/lab-tests">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Record Test'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2{grid-template-columns:1fr}}`]
})
export class LabTestFormComponent implements OnInit {
  private fb=inject(FormBuilder); private srv=inject(LabTestService); private pSrv=inject(ProjectService);
  private notify=inject(NotificationService); private route=inject(ActivatedRoute); private router=inject(Router);
  projects:any[]=[]; edit=false; id:string|null=null; saving=false;
  f=this.fb.group({projectId:['',Validators.required],testName:['',Validators.required],testCode:['',Validators.required],category:['Concrete'],testDate:[new Date(),Validators.required],labName:['',Validators.required],technicianName:['',Validators.required],testStandard:[''],parameterTested:['',Validators.required],specificationLimit:['']});

  ngOnInit(){
    this.pSrv.getProjects({page:1,pageSize:100}).subscribe((r:any)=>{if(r.success)this.projects=r.data});
    const iid=this.route.snapshot.paramMap.get('id'); if(iid){this.edit=true;this.id=iid;this.load(iid);}
  }

  load(id:string){this.saving=true;this.srv.getById(id).subscribe({next:(r:any)=>{if(r?.data){const d=r.data;this.f.patchValue({projectId:d.projectId,testName:d.testName||'',testCode:d.testCode||'',category:d.category||'Concrete',testDate:d.testDate?moment(d.testDate).toDate():new Date(),labName:d.labName||'',technicianName:d.technicianName||'',testStandard:d.testStandard||'',parameterTested:d.parameterTested||'',specificationLimit:d.specificationLimit||''})}this.saving=false},error:()=>{this.saving=false;this.router.navigate(['/lab-tests'])}})}

  submit(){
    if(this.f.invalid){this.f.markAllAsTouched();return;}
    this.saving=true;const v=this.f.getRawValue();
    const body:any={projectId:v.projectId,testName:(v.testName||'').trim(),testCode:(v.testCode||'').trim(),category:v.category||'Concrete',testDate:v.testDate?moment(v.testDate).toISOString():new Date().toISOString(),labName:(v.labName||'').trim(),technicianName:(v.technicianName||'').trim(),testStandard:(v.testStandard||'').trim(),parameterTested:(v.parameterTested||'').trim(),specificationLimit:(v.specificationLimit||'').trim()};
    const r$=this.edit&&this.id?this.srv.update(this.id,body):this.srv.create(body);
    r$.subscribe({next:(r:any)=>{this.saving=false;if(r.success){const u={...body,id:this.id||r.data?.id,projectName:r.data?.projectName||''};sessionStorage.setItem('labtest_updated',JSON.stringify(u));this.notify.success(this.edit?'Updated!':'Recorded!');this.router.navigate(['/lab-tests'])}else this.notify.error(r.message||'Failed')},error:(e:any)=>{this.saving=false;this.notify.error(e?.error?.message||'Failed')}});
  }
}