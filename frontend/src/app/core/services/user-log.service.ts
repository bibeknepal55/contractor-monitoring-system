import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';

export interface UserLogFilter extends PagedRequest {
  activityType?: string;
  moduleName?: string;
  userId?: string;
  ipAddress?: string;
  responseStatus?: number;
  startDate?: string;
  endDate?: string;
}

@Injectable({ providedIn: 'root' })
export class UserLogService {
  private api = inject(ApiService);
  private ep = '/user-logs';

  getLogs(params: UserLogFilter): Observable<PagedResponse<any>> {
    const queryParams: any = { ...params };
    return this.api.getPaged(this.ep, queryParams);
  }

  getLogDetail(id: string): Observable<ApiResponse<any>> {
    return this.api.get(`${this.ep}/${id}`);
  }

  getStats(): Observable<ApiResponse<any>> {
    return this.api.get(`${this.ep}/stats`);
  }

  getActiveSessions(): Observable<ApiResponse<any>> {
    return this.api.get(`${this.ep}/sessions/active`);
  }

  getUserLogs(userId: string, page: number = 1): Observable<PagedResponse<any>> {
    return this.api.getPaged(`${this.ep}/user/${userId}`, { page, pageSize: 20 });
  }

  purgeLogs(olderThanDays: number): Observable<ApiResponse<any>> {
    return this.api.delete(`${this.ep}/purge?olderThanDays=${olderThanDays}`);
  }
}