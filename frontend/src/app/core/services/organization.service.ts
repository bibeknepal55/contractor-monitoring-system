import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';
import { ResourceStore } from './resource-store';

export interface Organization {
  id: string;
  name: string;
  description: string;
  parentId?: string;
  type: 'Department' | 'Division' | 'Region' | 'Unit';
  isActive: boolean;
  userCount: number;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class OrganizationService {
  private api = inject(ApiService);
  private ep = '/organizations';
  private store = new ResourceStore<Organization>();
  public orgs$ = this.store.state$;

  getAll(req: PagedRequest): Observable<PagedResponse<Organization>> {
    return this.api.getPaged<Organization>(this.ep, req);
  }

  getById(id: string): Observable<ApiResponse<Organization>> {
    return this.api.get<Organization>(`${this.ep}/${id}`);
  }

  create(data: any): Observable<ApiResponse<Organization>> {
    return this.api.post<Organization>(this.ep, data);
  }

  update(id: string, data: any): Observable<ApiResponse<Organization>> {
    return this.api.put<Organization>(`${this.ep}/${id}`, data);
  }

  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.api.delete<boolean>(`${this.ep}/${id}`);
  }

  getUsers(id: string, req: PagedRequest): Observable<PagedResponse<any>> {
    return this.api.getPaged<any>(`${this.ep}/${id}/users`, req);
  }

  loadOrgs(params: PagedRequest): void {
    this.store.setLoading(true);
    this.getAll(params).subscribe({
      next: (r: any) => {
        if (r.success) this.store.setData(r.data, r.totalCount);
        else this.store.setError(r.message || 'Failed');
      },
      error: (e) => this.store.setError(e?.error?.message || 'Failed to load organizations')
    });
  }

  createAndRefresh(data: any): Observable<ApiResponse<Organization>> {
    const obs = this.create(data);
    obs.subscribe({ next: (r) => { if (r.success) this.loadOrgs({ page: 1, pageSize: 50 }); } });
    return obs;
  }

  updateAndRefresh(id: string, data: any): Observable<ApiResponse<Organization>> {
    const obs = this.update(id, data);
    obs.subscribe({ next: (r) => { if (r.success) this.loadOrgs({ page: 1, pageSize: 50 }); } });
    return obs;
  }

  deleteAndRefresh(id: string): Observable<ApiResponse<boolean>> {
    const obs = this.delete(id);
    obs.subscribe({ next: (r) => { if (r.success) this.loadOrgs({ page: 1, pageSize: 50 }); } });
    return obs;
  }

  clearStore(): void { this.store.clear(); }
}