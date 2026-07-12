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
import { MaterialService } from '../../../core/services/material.service';
import { ProjectService } from '../../../core/services/project.service';
import { NotificationService } from '../../../core/services/notification.service';
import moment from 'moment';

@Component({
  selector: 'app-material-form', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule, MatCardModule, RouterLink],
  template: `
    <div class="form-page">
      <div class="top-bar"><button mat-icon-button routerLink="/materials"><mat-icon>arrow_back</mat-icon></button><div><h1>{{edit?'Edit':'Add'}} Raw Material</h1><p>Track construction materials procurement</p></div></div>
      <form [formGroup]="f" (ngSubmit)="submit()">
        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>business</mat-icon><mat-card-title>Project & Material</mat-card-title></mat-card-header><mat-card-content>
          <mat-form-field appearance="outline"><mat-label>Project *</mat-label><mat-select formControlName="projectId"><mat-option *ngFor="let p of projects" [value]="p.id">{{p.projectName}} ({{p.projectCode}})</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Material Code *</mat-label><input matInput formControlName="materialCode" placeholder="e.g., CEM-001"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Material Name *</mat-label><input matInput formControlName="materialName" placeholder="Enter material name"><mat-error>Required</mat-error></mat-form-field>
          </div>
          <mat-form-field appearance="outline"><mat-label>Category</mat-label><mat-select formControlName="category"><mat-option value="Cement">Cement</mat-option><mat-option value="Steel">Steel</mat-option><mat-option value="Aggregate">Aggregate</mat-option><mat-option value="Sand">Sand</mat-option><mat-option value="Bricks">Bricks</mat-option><mat-option value="Electrical">Electrical</mat-option><mat-option value="Plumbing">Plumbing</mat-option><mat-option value="Other">Other</mat-option></mat-select></mat-form-field>
        </mat-card-content></mat-card>

        <mat-card class="card"><mat-card-header><mat-icon mat-card-avatar>shopping_cart</mat-icon><mat-card-title>Order Details</mat-card-title></mat-card-header><mat-card-content>
          <div class="r3">
            <mat-form-field appearance="outline"><mat-label>Quantity Ordered *</mat-label><input matInput type="number" formControlName="quantityOrdered" placeholder="0"><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Unit *</mat-label><mat-select formControlName="unit"><mat-option value="nos">Nos</mat-option><mat-option value="kg">Kg</mat-option><mat-option value="MT">Metric Ton</mat-option><mat-option value="cum">Cubic Meter</mat-option><mat-option value="sqm">Sq. Meter</mat-option><mat-option value="bags">Bags</mat-option><mat-option value="liters">Liters</mat-option></mat-select><mat-error>Required</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Unit Price (₹) *</mat-label><input matInput type="number" formControlName="unitPrice" placeholder="0"><mat-error>Required</mat-error></mat-form-field>
          </div>
          <div class="total" *ngIf="f.get('quantityOrdered')?.value && f.get('unitPrice')?.value">
            <mat-icon>calculate</mat-icon><span>Total Value: <strong>₹{{((f.get('quantityOrdered')?.value||0)*(f.get('unitPrice')?.value||0))|number:'1.2-2'}}</strong></span>
          </div>
          <div class="r2">
            <mat-form-field appearance="outline"><mat-label>Supplier Name</mat-label><input matInput formControlName="supplierName" placeholder="Supplier name"></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Order Date *</mat-label><input matInput [matDatepicker]="d1" formControlName="orderDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker><mat-error>Required</mat-error></mat-form-field>
          </div>
        </mat-card-content></mat-card>

        <div class="btns"><button mat-stroked-button type="button" routerLink="/materials">Cancel</button><button mat-flat-button color="primary" type="submit" [disabled]="saving||f.invalid">{{edit?'Update':'Add Material'}}</button></div>
      </form>
    </div>
  `,
  styles: [`.form-page{max-width:900px;margin:0 auto;padding:24px}.top-bar{display:flex;align-items:center;gap:16px;margin-bottom:24px}.top-bar h1{font-size:1.5rem;font-weight:700;margin:0}.top-bar p{margin:2px 0 0;color:#666}.card{border-radius:12px;box-shadow:0 1px 4px rgba(0,0,0,.08);margin-bottom:20px}.card mat-card-header{padding:16px 20px 0}.card mat-card-content{padding:16px 20px 20px}.r2{display:grid;grid-template-columns:1fr 1fr;gap:16px}.r3{display:grid;grid-template-columns:1fr 1fr 1fr;gap:16px}mat-form-field{width:100%}.total{display:flex;align-items:center;gap:8px;padding:12px 16px;background:#e8f5e9;border-radius:8px;margin-bottom:16px;color:#137333;font-size:.9rem}.total mat-icon{color:#137333}.btns{display:flex;justify-content:flex-end;gap:12px;padding-bottom:24px}.btns button{min-width:130px;height:44px}@media(max-width:768px){.r2,.r3{grid-template-columns:1fr}}`]
})
export class MaterialFormComponent implements OnInit {
  private fb=inject(FormBuilder); private srv=inject(MaterialService); private pSrv=inject(ProjectService);
  private notify=inject(NotificationService); private route=inject(ActivatedRoute); private router=inject(Router);
  projects:any[]=[]; edit=false; id:string|null=null; saving=false;
  f=this.fb.group({projectId:['',Validators.required],materialName:['',Validators.required],materialCode:['',Validators.required],category:['Cement'],quantityOrdered:[0,[Validators.required,Validators.min(0)]],unit:['nos',Validators.required],unitPrice:[0,[Validators.required,Validators.min(0)]],supplierName:[''],orderDate:[new Date(),Validators.required]});

  ngOnInit(){
    this.pSrv.getProjects({page:1,pageSize:100}).subscribe((r:any)=>{if(r.success)this.projects=r.data});
    const iid=this.route.snapshot.paramMap.get('id'); if(iid){this.edit=true;this.id=iid;this.load(iid);}
  }

  load(id:string){this.saving=true;this.srv.getById(id).subscribe({next:(r:any)=>{if(r?.data){const d=r.data;this.f.patchValue({projectId:d.projectId,materialName:d.materialName||'',materialCode:d.materialCode||'',category:d.category||'Cement',quantityOrdered:d.quantityOrdered||0,unit:d.unit||'nos',unitPrice:d.unitPrice||0,supplierName:d.supplierName||'',orderDate:d.orderDate?moment(d.orderDate).toDate():new Date()})}this.saving=false},error:()=>{this.saving=false;this.router.navigate(['/materials'])}})}

  submit(){
    if(this.f.invalid){this.f.markAllAsTouched();return;}
    this.saving=true;const v=this.f.getRawValue();
    const body:any={projectId:v.projectId,materialName:(v.materialName||'').trim(),materialCode:(v.materialCode||'').trim(),category:v.category||'Cement',quantityOrdered:Number(v.quantityOrdered)||0,unit:v.unit||'nos',unitPrice:Number(v.unitPrice)||0,supplierName:(v.supplierName||'').trim(),orderDate:v.orderDate?moment(v.orderDate).toISOString():null};
    const r$=this.edit&&this.id?this.srv.update(this.id,body):this.srv.create(body);
    r$.subscribe({next:(r:any)=>{this.saving=false;if(r.success){const u={...body,id:this.id||r.data?.id,projectName:r.data?.projectName||''};sessionStorage.setItem('material_updated',JSON.stringify(u));this.notify.success(this.edit?'Updated!':'Added!');this.router.navigate(['/materials'])}else this.notify.error(r.message||'Failed')},error:(e:any)=>{this.saving=false;this.notify.error(e?.error?.message||'Failed')}});
  }
}