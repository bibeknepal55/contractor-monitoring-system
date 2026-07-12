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
import { PhotoService } from '../../../core/services/photo.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PhotoUploadComponent } from '../photo-upload/photo-upload.component';
import moment from 'moment';

@Component({
  selector: 'app-photo-form', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink, PhotoUploadComponent],
  template: `
    <div class="form-page">
      <div class="top-bar"><button mat-icon-button routerLink="/photos"><mat-icon>arrow_back</mat-icon></button><div><h1>{{edit?'Edit':'Add'}} Photo Record</h1><p>Document site progress with photographic evidence</p></div></div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <!-- Upload Section -->
        <app-photo-upload (photoUploaded)="onPhotoUploaded($event)"></app-photo-upload>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Photo Info</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Title *</mat-label><input matInput formControlName="title" placeholder="e.g., Foundation Progress - Block A"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Photo Type</mat-label><mat-select formControlName="photoType"><mat-option value="Site">Site Overview</mat-option><mat-option value="Progress">Progress Shot</mat-option><mat-option value="Issue">Issue/Defect</mat-option><mat-option value="Completion">Completion</mat-option><mat-option value="General">General</mat-option></mat-select></mat-form-field>
          </div>
          <mat-form-field appearance="outline"><mat-label>Description</mat-label><textarea matInput formControlName="description" rows="2" placeholder="Describe what the photo shows"></textarea></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Photo Path *</mat-label><input matInput formControlName="photoPath" placeholder="Auto-filled after upload" readonly><mat-hint>Upload a photo above or enter path manually</mat-hint><mat-error>Photo path is required</mat-error></mat-form-field>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>location_on</mat-icon><mat-card-title>Location & Direction</mat-card-title></mat-card-header><mat-card-content>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Location *</mat-label><input matInput formControlName="location" placeholder="e.g., Block A - Ground Floor"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Direction</mat-label><mat-select formControlName="direction"><mat-option value="North">North ↑</mat-option><mat-option value="South">South ↓</mat-option><mat-option value="East">East →</mat-option><mat-option value="West">West ←</mat-option><mat-option value="North-East">North-East ↗</mat-option><mat-option value="North-West">North-West ↖</mat-option><mat-option value="South-East">South-East ↘</mat-option><mat-option value="South-West">South-West ↙</mat-option></mat-select></mat-form-field>
          </div>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Photo Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="photoDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Tags</mat-label><input matInput formControlName="tags" placeholder="e.g., foundation, concrete, progress"><mat-hint>Comma-separated tags</mat-hint></mat-form-field>
          </div>
        </mat-card-content></mat-card>

        <div class="btns"><button mat-stroked-button type="button" routerLink="/photos">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Add Photo'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}mat-form-field{width:100%}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2{grid-template-columns:1fr}}`]
})
export class PhotoFormComponent implements OnInit {
  private fb=inject(FormBuilder); private srv=inject(PhotoService); private pSrv=inject(ProjectService);
  private notify=inject(NotificationService); private route=inject(ActivatedRoute); private router=inject(Router);
  projects:any[]=[]; edit=false; id:string|null=null; saving=false;
  f=this.fb.group({projectId:['',Validators.required],title:['',Validators.required],description:[''],photoPath:['',Validators.required],photoDate:[new Date(),Validators.required],location:['',Validators.required],direction:['North'],photoType:['Site'],tags:['']});

  ngOnInit(){
    this.pSrv.getProjects({page:1,pageSize:100}).subscribe((r:any)=>{if(r.success)this.projects=r.data});
    const iid=this.route.snapshot.paramMap.get('id'); if(iid){this.edit=true;this.id=iid;this.load(iid);}
  }

  onPhotoUploaded(event: { path: string; filename: string }): void {
    if (event.path) {
      this.f.patchValue({ photoPath: event.path });
    }
  }

  load(id:string){this.saving=true;this.srv.getById(id).subscribe({next:(r:any)=>{if(r?.data){const d=r.data;this.f.patchValue({projectId:d.projectId,title:d.title||'',description:d.description||'',photoPath:d.photoPath||'',photoDate:d.photoDate?moment(d.photoDate).toDate():new Date(),location:d.location||'',direction:d.direction||'North',photoType:d.photoType||'Site',tags:d.tags||''})}this.saving=false},error:()=>{this.saving=false;this.router.navigate(['/photos'])}})}

  submit(){
    if(this.f.invalid){this.f.markAllAsTouched();return;}
    this.saving=true;const v=this.f.getRawValue();
    const body:any={projectId:v.projectId,title:(v.title||'').trim(),description:(v.description||'').trim(),photoPath:(v.photoPath||'').trim(),photoDate:v.photoDate?moment(v.photoDate).toISOString():new Date().toISOString(),location:(v.location||'').trim(),direction:v.direction||'North',photoType:v.photoType||'Site',tags:(v.tags||'').trim()};
    const r$=this.edit&&this.id?this.srv.update(this.id,body):this.srv.create(body);
    r$.subscribe({next:(r:any)=>{this.saving=false;if(r.success){const u={...body,id:this.id||r.data?.id,projectName:r.data?.projectName||''};sessionStorage.setItem('photo_updated',JSON.stringify(u));this.notify.success(this.edit?'Updated!':'Added!');this.router.navigate(['/photos'])}else this.notify.error(r.message||'Failed')},error:(e:any)=>{this.saving=false;this.notify.error(e?.error?.message||'Failed')}});
  }
}