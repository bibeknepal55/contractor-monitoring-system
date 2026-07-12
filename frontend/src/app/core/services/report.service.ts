import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private api = inject(ApiService);
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  generate(d: any): Observable<ApiResponse<any>> {
    return this.api.post('/reports/generate', d);
  }

  exportReport(d: any): Observable<Blob> {
    return this.http.post(`${this.base}/export/generate`, d, {
      responseType: 'blob'
    });
  }
}