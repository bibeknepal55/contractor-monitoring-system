import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class FinancialService {
  private api = inject(ApiService);
  private ep = '/contract-financials';

  getAll(req: PagedRequest): Observable<PagedResponse<any>> {
    return this.api.getPaged(this.ep, req);
  }
  getById(id: string): Observable<ApiResponse<any>> {
    return this.api.get(`${this.ep}/${id}`);
  }
  create(d: any): Observable<ApiResponse<any>> {
    return this.api.post(this.ep, d);
  }
  update(id: string, d: any): Observable<ApiResponse<any>> {
    return this.api.put(`${this.ep}/${id}`, d);
  }
  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.api.delete(`${this.ep}/${id}`);
  }
}