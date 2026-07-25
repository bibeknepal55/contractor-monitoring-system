import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private api = inject(ApiService);
  private ep = '/users';

  getUsers(req: any): Observable<PagedResponse<any>> {
    return this.api.getPaged(this.ep, req);
  }

  getUserById(id: string): Observable<ApiResponse<any>> {
    return this.api.get(`${this.ep}/${id}`);
  }

  createUser(data: any): Observable<ApiResponse<any>> {
    return this.api.post(this.ep, data);
  }

  updateUserRoles(id: string, roles: string[]): Observable<ApiResponse<any>> {
    return this.api.put(`${this.ep}/${id}/roles`, { roles });
  }

  updateUserStatus(id: string, isActive: boolean): Observable<ApiResponse<any>> {
    return this.api.put(`${this.ep}/${id}/status`, { isActive });
  }

  deleteUser(id: string): Observable<ApiResponse<any>> {
    return this.api.delete(`${this.ep}/${id}`);
  }

  getRoles(): Observable<ApiResponse<any>> {
    return this.api.get(`${this.ep}/roles`);
  }

  updateRolePermissions(roleId: string, permissions: string[]): Observable<ApiResponse<any>> {
    return this.api.put(`${this.ep}/roles/${roleId}/permissions`, { permissions });
  }
}