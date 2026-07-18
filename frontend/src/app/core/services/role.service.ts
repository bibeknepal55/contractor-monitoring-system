import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private api = inject(ApiService);
  private ep = '/roles';

  getAll(): Observable<ApiResponse<any[]>> { return this.api.get(this.ep); }
  getById(id: string): Observable<ApiResponse<any>> { return this.api.get(`${this.ep}/${id}`); }
  getModulePermissions(): Observable<ApiResponse<any[]>> { return this.api.get(`${this.ep}/modules/permissions`); }
  create(data: any): Observable<ApiResponse<any>> { return this.api.post(this.ep, data); }
  update(id: string, data: any): Observable<ApiResponse<any>> { return this.api.put(`${this.ep}/${id}`, data); }
  delete(id: string): Observable<ApiResponse<any>> { return this.api.delete(`${this.ep}/${id}`); }
}