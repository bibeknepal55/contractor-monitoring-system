import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProjectService } from '../../core/services/project.service';
import { ContractorService } from '../../core/services/contractor.service';
import { FinancialService } from '../../core/services/financial.service';
import { PriceAdjustmentService } from '../../core/services/price-adjustment.service';
import { BondService } from '../../core/services/bond.service';
import { GuaranteeService } from '../../core/services/guarantee.service';
import { ProgressService } from '../../core/services/progress.service';
import { TimeExtensionService } from '../../core/services/time-extension.service';
import { DelayService } from '../../core/services/delay.service';
import { MaterialService } from '../../core/services/material.service';
import { LabTestService } from '../../core/services/lab-test.service';
import { PhotoService } from '../../core/services/photo.service';
import { SubcontractorService } from '../../core/services/subcontractor.service';
import { OfficialService } from '../../core/services/official.service';
import { ApprovalService } from '../../core/services/approval.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { environment } from '../../../environments/environment';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import moment from 'moment';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule,
    MatCardModule, MatTableModule, MatProgressBarModule, MatDividerModule, MatTooltipModule
  ],
  template: `
    <div class="page">
      <div class="header">
        <div><h1>Reports & Analytics</h1><p>Generate and export detailed reports across all modules</p></div>
      </div>

      <div class="layout">
        <div class="sidebar">
          <mat-card class="filter-card">
            <mat-card-header><mat-icon mat-card-avatar style="color:#1a73e8;">tune</mat-icon><mat-card-title>Report Filters</mat-card-title></mat-card-header>
            <mat-divider></mat-divider>
            <mat-card-content>
              <form [formGroup]="filterForm">
                <mat-form-field appearance="outline"><mat-label>Report Type</mat-label><mat-select formControlName="reportType" (selectionChange)="onTypeChange()">
                  <mat-option value="project-wise">📋 Projects</mat-option>
                  <mat-option value="contractor-wise">🏢 Contractors</mat-option>
                  <mat-option value="financial">💰 Contract Financials</mat-option>
                  <mat-option value="price-adjustment">📈 Price Adjustments</mat-option>
                  <mat-option value="pb-apg">🔒 Bonds & APG</mat-option>
                  <mat-option value="progress">📊 Physical Progress</mat-option>
                  <mat-option value="time-extension">⏰ Time Extensions</mat-option>
                  <mat-option value="delay-analysis">⚠️ Delay Reasons</mat-option>
                  <mat-option value="material">🧱 Raw Materials</mat-option>
                  <mat-option value="lab-test">🔬 Lab Tests</mat-option>
                  <mat-option value="photo">📷 Photo Monitoring</mat-option>
                  <mat-option value="subcontractor">🤝 Subcontractors</mat-option>
                  <mat-option value="official">👤 Officials</mat-option>
                  <mat-option value="approval">✅ Approval Workflow</mat-option>
                </mat-select></mat-form-field>

                <div *ngIf="loadingRecords" style="text-align:center;padding:8px 0;"><mat-progress-bar mode="indeterminate"></mat-progress-bar></div>

                <mat-form-field appearance="outline" *ngIf="recordList.length > 0 && !loadingRecords"><mat-label>Select Record</mat-label><mat-select formControlName="recordId" (selectionChange)="onRecordChange()"><mat-option value="">All Records</mat-option><mat-option *ngFor="let r of recordList" [value]="r.id">{{r.name}}</mat-option></mat-select></mat-form-field>

                <div class="date-row">
                  <mat-form-field appearance="outline"><mat-label>Start Date</mat-label><input matInput [matDatepicker]="d1" formControlName="startDate"><mat-datepicker-toggle matSuffix [for]="d1"></mat-datepicker-toggle><mat-datepicker #d1></mat-datepicker></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>End Date</mat-label><input matInput [matDatepicker]="d2" formControlName="endDate"><mat-datepicker-toggle matSuffix [for]="d2"></mat-datepicker-toggle><mat-datepicker #d2></mat-datepicker></mat-form-field>
                </div>

                <mat-form-field appearance="outline"><mat-label>Status</mat-label><mat-select formControlName="status"><mat-option value="">All</mat-option><mat-option value="Active">Active</mat-option><mat-option value="Completed">Completed</mat-option><mat-option value="Pending">Pending</mat-option><mat-option value="Delayed">Delayed</mat-option></mat-select></mat-form-field>

                <button mat-flat-button color="primary" (click)="generate()" [disabled]="generating" class="gen-btn"><mat-icon>insights</mat-icon> Generate Report</button>
              </form>
            </mat-card-content>
          </mat-card>
        </div>

        <div class="content">
          <mat-progress-bar *ngIf="generating" mode="indeterminate" color="primary"></mat-progress-bar>

          <ng-container *ngIf="reportData && !generating">
            <div class="result-header">
              <div><h2>{{reportTitle}}</h2><span class="meta">{{reportData.length}} records</span></div>
              <div class="export-btns" *ngIf="reportData.length > 0 && auth.hasPermission('Reports.Export')">
                <button mat-stroked-button color="primary" (click)="exportExcel()"><mat-icon>table_chart</mat-icon> Excel</button>
                <button mat-stroked-button color="warn" (click)="exportPDF()"><mat-icon>picture_as_pdf</mat-icon> PDF</button>
              </div>
            </div>

            <div class="table-card" *ngIf="reportData.length > 0">
              <div class="table-scroll">
                <table mat-table [dataSource]="reportData">
                  <ng-container *ngFor="let col of displayColumns" [matColumnDef]="col.key">
                    <th mat-header-cell *matHeaderCellDef>{{col.label}}</th>
                    <td mat-cell *matCellDef="let row">{{formatCell(row[col.key], col.type)}}</td>
                  </ng-container>
                  <tr mat-header-row *matHeaderRowDef="columnKeys"></tr>
                  <tr mat-row *matRowDef="let row; columns: columnKeys"></tr>
                </table>
              </div>
            </div>

            <div class="empty-state" *ngIf="reportData.length === 0">
              <mat-icon>search_off</mat-icon><h3>No data found</h3><p>Try different filters</p>
            </div>
          </ng-container>

          <div class="empty-state" *ngIf="!generating && !reportData">
            <mat-icon>assessment</mat-icon><h3>Generate a Report</h3><p>Select a report type and click Generate</p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page{padding:24px;max-width:1500px;margin:0 auto}
    .header{margin-bottom:24px}.header h1{font-size:1.5rem;font-weight:700;margin:0}.header p{color:#666;font-size:.9rem}
    .layout{display:grid;grid-template-columns:370px 1fr;gap:24px;align-items:start}
    .filter-card{border-radius:12px;box-shadow:0 2px 8px rgba(0,0,0,.06);border:1px solid #e8eaed}
    .filter-card mat-card-header{padding:20px 20px 0}.filter-card mat-card-content{padding:16px 20px 20px}
    mat-divider{margin:12px 0}mat-form-field{width:100%;margin-bottom:8px}
    .date-row{display:grid;grid-template-columns:1fr 1fr;gap:12px}
    .gen-btn{width:100%;height:46px;font-weight:500;font-size:.95rem;margin-top:8px}
    .result-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:16px;flex-wrap:wrap;gap:12px}
    .result-header h2{font-size:1.2rem;font-weight:600;margin:0}.meta{font-size:.85rem;color:#666}
    .export-btns{display:flex;gap:8px}.export-btns button{font-weight:500}
    .table-card{background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.06);overflow:hidden}
    .table-scroll{overflow-x:auto}
    table{width:100%}th{background:#fafafa;font-weight:600;font-size:.78rem;text-transform:uppercase;color:#555;padding:12px 16px}td{padding:12px 16px;font-size:.85rem;color:#333}
    tr:hover td{background:#f8f9ff}
    .empty-state{text-align:center;padding:64px 20px;color:#888}.empty-state mat-icon{font-size:56px;width:56px;height:56px;color:#ccc;margin-bottom:12px}
    @media(max-width:1024px){.layout{grid-template-columns:1fr}}@media(max-width:600px){.page{padding:16px}.date-row{grid-template-columns:1fr}}
  `]
})
export class ReportsComponent implements OnInit {
  private http = inject(HttpClient);
  private pSrv = inject(ProjectService);
  private cSrv = inject(ContractorService);
  private fSrv = inject(FinancialService);
  private paSrv = inject(PriceAdjustmentService);
  private bSrv = inject(BondService);
  private gSrv = inject(GuaranteeService);
  private prSrv = inject(ProgressService);
  private teSrv = inject(TimeExtensionService);
  private dSrv = inject(DelayService);
  private mSrv = inject(MaterialService);
  private lSrv = inject(LabTestService);
  private phSrv = inject(PhotoService);
  private sSrv = inject(SubcontractorService);
  private oSrv = inject(OfficialService);
  private aSrv = inject(ApprovalService);
  readonly auth = inject(AuthService);
  private notify = inject(NotificationService);
  private fb = inject(FormBuilder);
  private base = environment.apiUrl;

  recordList: any[] = [];
  reportData: any[] | null = null;
  displayColumns: any[] = [];
  generating = false;
  loadingRecords = false;

  filterForm = this.fb.group({
    reportType: ['project-wise'],
    recordId: [''],
    startDate: [null as Date | null],
    endDate: [null as Date | null],
    status: [''],
  });

  get columnKeys(): string[] { return this.displayColumns.map((c: any) => c.key); }
  get reportTitle(): string {
    const t: Record<string, string> = {
      'project-wise': 'Projects Report', 'contractor-wise': 'Contractors Report',
      'financial': 'Contract Financials', 'price-adjustment': 'Price Adjustments',
      'pb-apg': 'Bonds & APG', 'progress': 'Physical Progress',
      'time-extension': 'Time Extensions', 'delay-analysis': 'Delay Analysis',
      'material': 'Raw Materials', 'lab-test': 'Lab Tests',
      'photo': 'Photo Monitoring', 'subcontractor': 'Subcontractors',
      'official': 'Responsible Officials', 'approval': 'Approval Workflow',
    };
    return t[this.filterForm.get('reportType')?.value || ''] || 'Report';
  }

  ngOnInit(): void { this.onTypeChange(); }

  onTypeChange(): void {
    const type = this.filterForm.get('reportType')?.value || '';
    this.recordList = [];
    this.filterForm.patchValue({ recordId: '', startDate: null, endDate: null });
    if (!type) return;
    this.loadingRecords = true;
    const obs = this.getServiceForType(type);
    if (!obs) { this.loadingRecords = false; return; }
    obs.subscribe({
      next: (resp: any) => {
        this.loadingRecords = false;
        if (resp?.data?.length > 0) {
          this.recordList = resp.data.map((item: any) => ({
            id: item.id,
            name: item.projectName || item.companyName || item.bondNumber || item.guaranteeNumber ||
                  item.testName || item.materialName || item.title || item.fullName || item.name ||
                  item.recordTitle || item.comments || (item.id ? item.id.substring(0, 8) + '...' : 'Unknown'),
            startDate: item.startDate || item.issueDate || item.requestDate || item.appointmentDate ||
                       item.orderDate || item.photoDate || item.testDate || item.progressDate ||
                       item.createdAt || item.submittedAt,
            endDate: item.endDate || item.expiryDate || item.completionDate
          }));
        }
      },
      error: () => { this.loadingRecords = false; }
    });
  }

  getServiceForType(type: string): any {
    const m: Record<string, any> = {
      'project-wise': this.pSrv, 'contractor-wise': this.cSrv, 'financial': this.fSrv,
      'price-adjustment': this.paSrv, 'pb-apg': this.bSrv, 'progress': this.prSrv,
      'time-extension': this.teSrv, 'delay-analysis': this.dSrv, 'material': this.mSrv,
      'lab-test': this.lSrv, 'photo': this.phSrv, 'subcontractor': this.sSrv,
      'official': this.oSrv, 'approval': this.aSrv,
    };
    const srv = m[type];
    if (!srv) return null;
    if (type === 'project-wise') return srv.getProjects({ page: 1, pageSize: 200 });
    if (type === 'contractor-wise') return srv.getContractors({ page: 1, pageSize: 200 });
    return srv.getAll({ page: 1, pageSize: 200 });
  }

  onRecordChange(): void {
    const rid = this.filterForm.get('recordId')?.value;
    if (!rid) { this.filterForm.patchValue({ startDate: null, endDate: null }); return; }
    const rec = this.recordList.find(r => r.id === rid);
    if (rec) {
      if (rec.startDate) this.filterForm.patchValue({ startDate: moment(rec.startDate).toDate() });
      if (rec.endDate) this.filterForm.patchValue({ endDate: moment(rec.endDate).toDate() });
    }
  }

  generate(): void {
    this.generating = true;
    this.reportData = null;
    const v = this.filterForm.getRawValue();
    const body: any = { reportType: v.reportType };
    if (v.startDate) body.startDate = moment(v.startDate).toISOString();
    if (v.endDate) body.endDate = moment(v.endDate).toISOString();
    if (v.recordId) {
      if (v.reportType === 'contractor-wise') body.contractorId = v.recordId;
      else body.projectId = v.recordId;
    }
    if (v.status) body.status = v.status;

    this.http.post(`${this.base}/reports/generate`, body).subscribe({
      next: (r: any) => {
        this.generating = false;
        if (r.success) {
          const items = Array.isArray(r.data) ? r.data : (r.data?.items || []);
          this.reportData = items;
          if (items.length > 0) { this.buildColumns(items); }
          else { this.displayColumns = []; }
        } else {
          this.reportData = [];
          this.displayColumns = [];
        }
      },
      error: (e: any) => { this.generating = false; this.notify.error(e?.error?.message || 'Failed'); }
    });
  }

  buildColumns(items: any[]): void {
    if (!items.length) { this.displayColumns = []; return; }
    const first = items[0];
    this.displayColumns = Object.keys(first)
      .filter(k => k !== 'id' && typeof first[k] !== 'object')
      .map(k => ({
        key: k,
        label: k.replace(/([A-Z])/g, ' $1').replace(/^./, (s: string) => s.toUpperCase()).trim(),
        type: typeof first[k] === 'number' ? 'number' :
              (k.toLowerCase().includes('date') || k.toLowerCase().includes('at') ? 'date' : 'text')
      }));
  }

  formatCell(value: any, type: string): string {
    if (value === null || value === undefined) return '-';
    if (type === 'date') return moment(value).format('DD/MM/YYYY');
    if (type === 'number') return '₹' + Number(value).toLocaleString('en-IN');
    return String(value);
  }

  // CLIENT-SIDE EXCEL EXPORT
  exportExcel(): void {
    if (!this.reportData || this.reportData.length === 0) return;

    // Prepare data with formatted values
    const exportData = this.reportData.map(row => {
      const obj: any = {};
      this.displayColumns.forEach(col => {
        obj[col.label] = this.formatCell(row[col.key], col.type);
      });
      return obj;
    });

    const worksheet = XLSX.utils.json_to_sheet(exportData);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Report');

    // Auto-size columns
    const colWidths = this.displayColumns.map(col => ({ wch: Math.max(col.label.length + 5, 15) }));
    worksheet['!cols'] = colWidths;

    const fileName = `${this.reportTitle.replace(/\s+/g, '_')}_${moment().format('YYYYMMDD_HHmm')}.xlsx`;
    XLSX.writeFile(workbook, fileName);
    this.notify.success('Excel report downloaded!');
  }

  // CLIENT-SIDE PDF EXPORT
  exportPDF(): void {
    if (!this.reportData || this.reportData.length === 0) return;

    const doc = new jsPDF('landscape', 'mm', 'a4');
    
    // Title
    doc.setFontSize(16);
    doc.setTextColor(26, 115, 232);
    doc.text(this.reportTitle, 14, 15);
    
    // Date
    doc.setFontSize(10);
    doc.setTextColor(100, 100, 100);
    doc.text(`Generated: ${moment().format('DD/MM/YYYY HH:mm')}`, 14, 22);
    doc.text(`Total Records: ${this.reportData.length}`, 14, 28);

    // Table columns
    const headers = this.displayColumns.map(col => col.label);
    const rows = this.reportData.map(row => 
      this.displayColumns.map(col => this.formatCell(row[col.key], col.type))
    );

    autoTable(doc, {
      head: [headers],
      body: rows,
      startY: 32,
      styles: {
        fontSize: 8,
        cellPadding: 3,
        lineColor: [200, 200, 200],
        lineWidth: 0.1,
      },
      headStyles: {
        fillColor: [26, 115, 232],
        textColor: [255, 255, 255],
        fontStyle: 'bold',
        fontSize: 9,
      },
      alternateRowStyles: {
        fillColor: [248, 249, 255],
      },
      margin: { top: 32 },
    });

    const fileName = `${this.reportTitle.replace(/\s+/g, '_')}_${moment().format('YYYYMMDD_HHmm')}.pdf`;
    doc.save(fileName);
    this.notify.success('PDF report downloaded!');
  }
}