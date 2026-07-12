import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';
import { ExecutiveDashboard } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly apiService = inject(ApiService);

  getExecutiveDashboard(): Observable<ApiResponse<ExecutiveDashboard>> {
    return this.apiService.get<ExecutiveDashboard>('/dashboard/executive');
  }
}