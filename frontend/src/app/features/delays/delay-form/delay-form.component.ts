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
import { DelayService } from '../../../core/services/delay.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-delay-form', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink],
  template: `
    <div class="form-page">
      <div class="top-bar"><button mat-icon-button routerLink="/delays"><mat-icon>arrow_back</mat-icon></button><div><h1>{{edit?'Edit':'Report'}} Delay Reason</h1><p>Document project delay with impact analysis</p></div></div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Category</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Delay Category *</mat-label><mat-select formControlName="delayCategory"><mat-option value="Weather">Weather</mat-option><mat-option value="Material">Material Shortage</mat-option><mat-option value="Labor">Labor Issues</mat-option><mat-option value="Equipment">Equipment Failure</mat-option><mat-option value="Design">Design Change</mat-option><mat-option value="Permit">Permit/Legal</mat-option><mat-option value="Other">Other</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Impact Level *</mat-label><mat-select formControlName="impactLevel"><mat-option value="Critical">Critical</mat-option><mat-option value="High">High</mat-option><mat-option value="Medium">Medium</mat-option><mat-option value="Low">Low</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          </div>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Delay Start Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="delayStartDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Delay End Date</mat-label><input matInput [matDatepicker]="d2" formControlName="delayEndDate"><mat-datepicker-toggle matSuffix [for]="d2"></mat-datepicker-toggle><mat-datepicker #d2></mat-datepicker></mat-form-field>
          </div>
          <div class="duration" *ngIf="f.get('delayStartDate')?.value && f.get('delayEndDate')?.value">
            <mat-icon>schedule</mat-icon><span>Duration: <strong>{{getDuration()}} days</strong></span>
          </div>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>description</mat-icon><mat-card-title>Details & Mitigation</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Description *</mat-label><textarea matInput formControlName="description" rows="3" placeholder="Detailed description of the delay"></textarea><mat-error>Required</mat-error></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Responsible Party</mat-label><input matInput formControlName="responsibleParty" placeholder="e.g., Contractor, Subcontractor, Weather"></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Mitigation Action</mat-label><textarea matInput formControlName="mitigationAction" rows="2" placeholder="Steps taken to mitigate the delay"></textarea></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Remarks</mat-label><textarea matInput formControlName="remarks" rows="2" placeholder="Additional notes"></textarea></mat-form-field>
        </mat-card-content></mat-card>
        <div class="btns"><button mat-stroked-button type="button" routerLink="/delays">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Report Delay'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.duration{display:flex;align-items:center;gap:8px;padding:12px 16px;background:#fff3e0;border-radius:8px;margin-top:8px;color:#e65100;font-size:.9rem}.duration mat-icon{color:#e65100}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2{grid-template-columns:1fr}}`]
})
export class DelayFormComponent implements OnInit {
  private fb=inject(FormBuilder); private srv=inject(DelayService); private pSrv=inject(ProjectService);
  private notify=inject(NotificationService); private route=inject(ActivatedRoute); private router=inject(Router);
  projects:any[]=[]; edit=false; id:string|null=null; saving=false;
  f=this.fb.group({projectId:['',Validators.required],delayCategory:['',Validators.required],description:['',Validators.required],delayStartDate:[null as Date|null,Validators.required],delayEndDate:[null as Date|null],impactLevel:['Medium',Validators.required],responsibleParty:[''],mitigationAction:[''],remarks:['']});

  ngOnInit(){
    this.pSrv.getProjects({page:1,pageSize:100}).subscribe((r:any)=>{if(r.success)this.projects=r.data});
    const iid=this.route.snapshot.paramMap.get('id'); if(iid){this.edit=true;this.id=iid;this.load(iid);}
  }

  getDuration():number{const s=this.f.get('delayStartDate')?.value;const e=this.f.get('delayEndDate')?.value;if(!s||!e)return 0;return moment(e).diff(moment(s),'days')}

  load(id:string){this.saving=true;this.srv.getById(id).subscribe({next:(r:any)=>{if(r?.data){const d=r.data;this.f.patchValue({projectId:d.projectId,delayCategory:d.delayCategory||'',description:d.description||'',delayStartDate:d.delayStartDate?moment(d.delayStartDate).toDate():null,delayEndDate:d.delayEndDate?moment(d.delayEndDate).toDate():null,impactLevel:d.impactLevel||'Medium',responsibleParty:d.responsibleParty||'',mitigationAction:d.mitigationAction||'',remarks:d.remarks||''})}this.saving=false},error:()=>{this.saving=false;this.router.navigate(['/delays'])}})}

  submit(){
    if(this.f.invalid){this.f.markAllAsTouched();return;}
    this.saving=true;const v=this.f.getRawValue();
    const body:any={projectId:v.projectId,delayCategory:v.delayCategory,description:(v.description||'').trim(),delayStartDate:v.delayStartDate?moment(v.delayStartDate).toISOString():null,delayEndDate:v.delayEndDate?moment(v.delayEndDate).toISOString():null,impactLevel:v.impactLevel,responsibleParty:(v.responsibleParty||'').trim(),mitigationAction:(v.mitigationAction||'').trim(),remarks:(v.remarks||'').trim()};
    const r$=this.edit&&this.id?this.srv.update(this.id,body):this.srv.create(body);
    r$.subscribe({next:(r:any)=>{this.saving=false;if(r.success){const u={...body,id:this.id||r.data?.id,projectName:r.data?.projectName||''};sessionStorage.setItem('delay_updated',JSON.stringify(u));this.notify.success(this.edit?'Updated!':'Reported!');this.router.navigate(['/delays'])}else this.notify.error(r.message||'Failed')},error:(e:any)=>{this.saving=false;this.notify.error(e?.error?.message||'Failed')}});
  }
}