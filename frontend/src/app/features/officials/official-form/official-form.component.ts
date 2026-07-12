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
import { OfficialService } from '../../../core/services/official.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-official-form', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink],
  template: `
    <div class="form-page">
      <div class="top-bar"><button mat-icon-button routerLink="/officials"><mat-icon>arrow_back</mat-icon></button><div><h1>{{edit?'Edit':'Add'}} Responsible Official</h1><p>Assign key personnel to projects</p></div></div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Personal Info</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Full Name *</mat-label><input matInput formControlName="fullName" placeholder="Official's full name"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Position *</mat-label><input matInput formControlName="position" placeholder="e.g., Senior Engineer"><mat-error>Required</mat-error></mat-form-field>
          </div>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Department</mat-label><input matInput formControlName="department" placeholder="e.g., Civil, Electrical"></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Organization</mat-label><input matInput formControlName="organization" placeholder="e.g., Ministry of Infrastructure"></mat-form-field>
          </div>
          <mat-form-field appearance="outline"><mat-label>Role *</mat-label><mat-select formControlName="role"><mat-option value="Project Manager">Project Manager</mat-option><mat-option value="Site Engineer">Site Engineer</mat-option><mat-option value="Quality Inspector">Quality Inspector</mat-option><mat-option value="Safety Officer">Safety Officer</mat-option><mat-option value="Coordinator">Coordinator</mat-option><mat-option value="Supervisor">Supervisor</mat-option><mat-option value="Member">Member</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>contact_phone</mat-icon><mat-card-title>Contact Details</mat-card-title></mat-card-header><mat-card-content>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Email *</mat-label><input matInput formControlName="email" type="email" placeholder="official@org.com"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Phone</mat-label><input matInput formControlName="phone" placeholder="+977-"></mat-form-field>
          </div>
          <mat-form-field appearance="outline"><mat-label>Mobile</mat-label><input matInput formControlName="mobile" placeholder="+977-98XXXXXXXX"></mat-form-field>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>school</mat-icon><mat-card-title>Qualifications & Experience</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Qualifications</mat-label><textarea matInput formControlName="qualifications" rows="2" placeholder="e.g., B.E. Civil, M.Sc. Structural Engineering"></textarea></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Years of Experience</mat-label><input matInput type="number" formControlName="yearsOfExperience" placeholder="0" min="0"></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Appointment Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="appointmentDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
          </div>
        </mat-card-content></mat-card>

        <div class="btns"><button mat-stroked-button type="button" routerLink="/officials">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Add Official'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2{grid-template-columns:1fr}}`]
})
export class OfficialFormComponent implements OnInit {
  private fb=inject(FormBuilder); private srv=inject(OfficialService); private pSrv=inject(ProjectService);
  private notify=inject(NotificationService); private route=inject(ActivatedRoute); private router=inject(Router);
  projects:any[]=[]; edit=false; id:string|null=null; saving=false;
  f=this.fb.group({projectId:['',Validators.required],fullName:['',Validators.required],position:['',Validators.required],department:[''],organization:[''],email:['',[Validators.required,Validators.email]],phone:[''],mobile:[''],role:['Member',Validators.required],appointmentDate:[new Date(),Validators.required],qualifications:[''],yearsOfExperience:[0,[Validators.min(0)]]});

  ngOnInit(){
    this.pSrv.getProjects({page:1,pageSize:100}).subscribe((r:any)=>{if(r.success)this.projects=r.data});
    const iid=this.route.snapshot.paramMap.get('id'); if(iid){this.edit=true;this.id=iid;this.load(iid);}
  }

  load(id:string){this.saving=true;this.srv.getById(id).subscribe({next:(r:any)=>{if(r?.data){const d=r.data;this.f.patchValue({projectId:d.projectId,fullName:d.fullName||'',position:d.position||'',department:d.department||'',organization:d.organization||'',email:d.email||'',phone:d.phone||'',mobile:d.mobile||'',role:d.role||'Member',appointmentDate:d.appointmentDate?moment(d.appointmentDate).toDate():new Date(),qualifications:d.qualifications||'',yearsOfExperience:d.yearsOfExperience||0})}this.saving=false},error:()=>{this.saving=false;this.router.navigate(['/officials'])}})}

  submit(){
    if(this.f.invalid){this.f.markAllAsTouched();return;}
    this.saving=true;const v=this.f.getRawValue();
    const body:any={projectId:v.projectId,fullName:(v.fullName||'').trim(),position:(v.position||'').trim(),department:(v.department||'').trim(),organization:(v.organization||'').trim(),email:(v.email||'').trim(),phone:(v.phone||'').trim(),mobile:(v.mobile||'').trim(),role:v.role||'Member',appointmentDate:v.appointmentDate?moment(v.appointmentDate).toISOString():new Date().toISOString(),qualifications:(v.qualifications||'').trim(),yearsOfExperience:Number(v.yearsOfExperience)||0};
    const r$=this.edit&&this.id?this.srv.update(this.id,body):this.srv.create(body);
    r$.subscribe({next:(r:any)=>{this.saving=false;if(r.success){const u={...body,id:this.id||r.data?.id,projectName:r.data?.projectName||''};sessionStorage.setItem('official_updated',JSON.stringify(u));this.notify.success(this.edit?'Updated!':'Added!');this.router.navigate(['/officials'])}else this.notify.error(r.message||'Failed')},error:(e:any)=>{this.saving=false;this.notify.error(e?.error?.message||'Failed')}});
  }
}