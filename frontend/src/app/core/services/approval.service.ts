import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private api = inject(ApiService);
  private ep = '/approvals';

  getAll(req: PagedRequest): Observable<PagedResponse<any>> { return this.api.getPaged(this.ep, req); }
  submit(d: any): Observable<ApiResponse<any>> { return this.api.post(`${this.ep}/submit`, d); }
  update(id: string, d: any): Observable<ApiResponse<any>> { return this.api.put(`${this.ep}/${id}`, d); }
  process(id: string, d: any): Observable<ApiResponse<any>> { return this.api.put(`${this.ep}/${id}/process`, d); }
}