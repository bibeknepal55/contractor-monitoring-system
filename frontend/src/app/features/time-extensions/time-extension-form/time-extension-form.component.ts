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
import { TimeExtensionService } from '../../../core/services/time-extension.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-time-extension-form', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink],
  template: `
    <div class="form-page">
      <div class="top-bar"><button mat-icon-button routerLink="/time-extensions"><mat-icon>arrow_back</mat-icon></button><div><h1>{{edit?'Edit':'Request'}} Time Extension</h1><p>Extend project completion timeline</p></div></div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Request</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Request Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="requestDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Days Requested *</mat-label><input matInput type="number" formControlName="daysRequested" placeholder="0" min="1"><mat-error>Required</mat-error></mat-form-field>
          </div>
          <mat-form-field appearance="outline"><mat-label>Original Completion Date *</mat-label><input matInput [matDatepicker]="d2" formControlName="originalCompletionDate"><mat-datepicker-toggle matSuffix [for]="d2"></mat-datepicker-toggle><mat-datepicker #d2></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
          <div class="preview" *ngIf="f.get('originalCompletionDate')?.value && f.get('daysRequested')?.value">
            <mat-icon>event</mat-icon>
            <span>New Completion Date: <strong>{{getNewDate()}}</strong></span>
          </div>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>description</mat-icon><mat-card-title>Justification</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Reason *</mat-label><textarea matInput formControlName="reason" rows="3" placeholder="Detailed reason for extension"></textarea><mat-error>Required</mat-error></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Remarks</mat-label><textarea matInput formControlName="remarks" rows="2" placeholder="Additional remarks"></textarea></mat-form-field>
        </mat-card-content></mat-card>

        <div class="btns"><button mat-stroked-button type="button" routerLink="/time-extensions">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Submit Request'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.preview{display:flex;align-items:center;gap:8px;padding:12px 16px;background:#e8f5e9;border-radius:8px;margin-top:8px;color:#137333;font-size:.9rem}.preview mat-icon{color:#137333}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2{grid-template-columns:1fr}}`]
})
export class TimeExtensionFormComponent implements OnInit {
  private fb=inject(FormBuilder); private srv=inject(TimeExtensionService); private pSrv=inject(ProjectService);
  private notify=inject(NotificationService); private route=inject(ActivatedRoute); private router=inject(Router);
  projects:any[]=[]; edit=false; id:string|null=null; saving=false;
  f=this.fb.group({projectId:['',Validators.required],requestDate:[new Date(),Validators.required],daysRequested:[0,[Validators.required,Validators.min(1)]],originalCompletionDate:[null as Date|null,Validators.required],reason:['',Validators.required],remarks:['']});

  ngOnInit(){
    this.pSrv.getProjects({page:1,pageSize:100}).subscribe((r:any)=>{if(r.success)this.projects=r.data});
    const iid=this.route.snapshot.paramMap.get('id'); if(iid){this.edit=true;this.id=iid;this.load(iid);}
  }

  getNewDate():string{const d=this.f.get('originalCompletionDate')?.value;const days=this.f.get('daysRequested')?.value;if(!d||!days)return'-';return moment(d).add(days,'days').format('DD/MM/YYYY')}

  load(id:string){this.saving=true;this.srv.getById(id).subscribe({next:(r:any)=>{if(r?.data){const d=r.data;this.f.patchValue({projectId:d.projectId,requestDate:d.requestDate?moment(d.requestDate).toDate():new Date(),daysRequested:d.daysRequested||0,originalCompletionDate:d.originalCompletionDate?moment(d.originalCompletionDate).toDate():null,reason:d.reason||'',remarks:d.remarks||''})}this.saving=false},error:()=>{this.saving=false;this.router.navigate(['/time-extensions'])}})}

  submit(){
    if(this.f.invalid){this.f.markAllAsTouched();return;}
    this.saving=true;const v=this.f.getRawValue();
    const body:any={projectId:v.projectId,requestDate:v.requestDate?moment(v.requestDate).toISOString():null,daysRequested:Number(v.daysRequested)||0,originalCompletionDate:v.originalCompletionDate?moment(v.originalCompletionDate).toISOString():null,reason:(v.reason||'').trim(),remarks:(v.remarks||'').trim()};
    const r$=this.edit&&this.id?this.srv.update(this.id,body):this.srv.create(body);
    r$.subscribe({next:(r:any)=>{this.saving=false;if(r.success){const u={...body,id:this.id||r.data?.id,projectName:r.data?.projectName||''};sessionStorage.setItem('te_updated',JSON.stringify(u));this.notify.success(this.edit?'Updated!':'Submitted!');this.router.navigate(['/time-extensions'])}else this.notify.error(r.message||'Failed')},error:(e:any)=>{this.saving=false;this.notify.error(e?.error?.message||'Failed')}});
  }
}