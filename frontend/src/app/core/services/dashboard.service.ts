import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getExecutiveDashboard(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/dashboard/executive`);
  }

  getDepartmentDashboard(): Observable<ApiResponse<any>> {
    // Routes to executive dashboard — department-level filtering is done server-side via TenantId claim
    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/dashboard/executive`);
  }

  getStats(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/dashboard/executive`);
  }

  getRecentActivity(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/user-logs?page=1&pageSize=10`);
  }
}