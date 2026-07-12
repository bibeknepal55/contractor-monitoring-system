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
import { MatSliderModule } from '@angular/material/slider';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProgressService } from '../../../core/services/progress.service';
import { ProjectService } from '../../../core/services/project.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-progress-form', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, MatSliderModule, RouterLink],
  template: `
    <div class="form-page">
      <div class="top-bar"><button mat-icon-button routerLink="/progress"><mat-icon>arrow_back</mat-icon></button><div><h1>{{edit?'Edit':'Record'}} Physical Progress</h1><p>Track on-site construction progress</p></div></div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Date</mat-card-title></mat-card-header><mat-card-content>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Progress Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="progressDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
          </div>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>trending_up</mat-icon><mat-card-title>Progress Metrics</mat-card-title></mat-card-header><mat-card-content>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Planned Progress (%)</mat-label><input matInput type="number" formControlName="plannedProgress" min="0" max="100"><mat-error>Required (0-100)</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Actual Progress (%)</mat-label><input matInput type="number" formControlName="actualProgress" min="0" max="100"><mat-error>Required (0-100)</mat-error></mat-form-field>
          </div>
          <div class="gauge" *ngIf="f.get('plannedProgress')?.value || f.get('actualProgress')?.value">
            <div class="gauge-item"><span>Planned</span><div class="bar-bg"><div class="bar-fill planned" [style.width.%]="f.get('plannedProgress')?.value||0"></div></div><strong>{{f.get('plannedProgress')?.value||0}}%</strong></div>
            <div class="gauge-item"><span>Actual</span><div class="bar-bg"><div class="bar-fill actual" [style.width.%]="f.get('actualProgress')?.value||0" [style.background]="(f.get('actualProgress')?.value||0)>=(f.get('plannedProgress')?.value||0)?'#137333':'#c5221f'"></div></div><strong [style.color]="(f.get('actualProgress')?.value||0)>=(f.get('plannedProgress')?.value||0)?'#137333':'#c5221f'">{{f.get('actualProgress')?.value||0}}%</strong></div>
          </div>
          <mat-form-field appearance="outline"><mat-label>Status</mat-label><mat-select formControlName="status"><mat-option value="On Track">On Track</mat-option><mat-option value="Delayed">Delayed</mat-option><mat-option value="At Risk">At Risk</mat-option><mat-option value="Completed">Completed</mat-option></mat-select></mat-form-field>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>description</mat-icon><mat-card-title>Details</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Activity Description *</mat-label><textarea matInput formControlName="activityDescription" rows="2" placeholder="Describe the activity"></textarea><mat-error>Required</mat-error></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Bottlenecks</mat-label><textarea matInput formControlName="bottlenecks" rows="2" placeholder="Any issues or bottlenecks"></textarea></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Mitigation Plan</mat-label><textarea matInput formControlName="mitigationPlan" rows="2" placeholder="Plan to address bottlenecks"></textarea></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Reported By *</mat-label><input matInput formControlName="reportedBy" placeholder="Name of reporter"><mat-error>Required</mat-error></mat-form-field>
        </mat-card-content></mat-card>

        <div class="btns"><button mat-stroked-button type="button" routerLink="/progress">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Record'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.gauge{background:#f8f9fa;border-radius:8px;padding:16px;margin-bottom:16px}.gauge-item{display:flex;align-items:center;gap:12px;margin-bottom:10px}.gauge-item span{width:60px;font-size:.85rem;color:#666}.bar-bg{flex:1;height:10px;background:#e0e0e0;border-radius:5px;overflow:hidden}.bar-fill{height:100%;border-radius:5px;transition:width .3s}.bar-fill.planned{background:#1976d2}.bar-fill.actual{background:#137333}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2{grid-template-columns:1fr}}`]
})
export class ProgressFormComponent implements OnInit {
  private fb=inject(FormBuilder); private srv=inject(ProgressService); private pSrv=inject(ProjectService);
  private auth=inject(AuthService); private notify=inject(NotificationService); private route=inject(ActivatedRoute); private router=inject(Router);
  projects:any[]=[]; edit=false; id:string|null=null; saving=false;
  f=this.fb.group({projectId:['',Validators.required],progressDate:[null as Date|null,Validators.required],plannedProgress:[0,[Validators.required,Validators.min(0),Validators.max(100)]],actualProgress:[0,[Validators.required,Validators.min(0),Validators.max(100)]],activityDescription:['',Validators.required],bottlenecks:[''],mitigationPlan:[''],reportedBy:['',Validators.required],status:['On Track']});

  ngOnInit(){
    this.pSrv.getProjects({page:1,pageSize:100}).subscribe((r:any)=>{if(r.success)this.projects=r.data});
    const u=this.auth.getCurrentUser(); if(u) this.f.patchValue({reportedBy:u.firstName+' '+u.lastName});
    const iid=this.route.snapshot.paramMap.get('id'); if(iid){this.edit=true;this.id=iid;this.load(iid);}
  }

  load(id:string){this.saving=true;this.srv.getById(id).subscribe({next:(r:any)=>{if(r?.data){const d=r.data;this.f.patchValue({projectId:d.projectId,progressDate:d.progressDate?moment(d.progressDate).toDate():null,plannedProgress:d.plannedProgress||0,actualProgress:d.actualProgress||0,activityDescription:d.activityDescription||'',bottlenecks:d.bottlenecks||'',mitigationPlan:d.mitigationPlan||'',reportedBy:d.reportedBy||'',status:d.status||'On Track'})}this.saving=false},error:()=>{this.saving=false;this.router.navigate(['/progress'])}})}

  submit(){
    if(this.f.invalid){this.f.markAllAsTouched();return;}
    this.saving=true;const v=this.f.getRawValue();
    const body:any={projectId:v.projectId,progressDate:v.progressDate?moment(v.progressDate).toISOString():null,plannedProgress:Number(v.plannedProgress)||0,actualProgress:Number(v.actualProgress)||0,activityDescription:(v.activityDescription||'').trim(),bottlenecks:(v.bottlenecks||'').trim(),mitigationPlan:(v.mitigationPlan||'').trim(),reportedBy:(v.reportedBy||'').trim(),status:v.status||'On Track'};
    const r$=this.edit&&this.id?this.srv.update(this.id,body):this.srv.create(body);
    r$.subscribe({next:(r:any)=>{this.saving=false;if(r.success){const u={...body,id:this.id||r.data?.id,projectName:r.data?.projectName||''};sessionStorage.setItem('progress_updated',JSON.stringify(u));this.notify.success(this.edit?'Updated!':'Recorded!');this.router.navigate(['/progress'])}else this.notify.error(r.message||'Failed')},error:(e:any)=>{this.saving=false;this.notify.error(e?.error?.message||'Failed')}});
  }
}