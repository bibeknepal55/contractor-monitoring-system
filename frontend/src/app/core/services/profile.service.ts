import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private api = inject(ApiService);
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  getProfile(): Observable<ApiResponse<any>> {
    return this.api.get('/profile');
  }

  updateProfile(data: any): Observable<ApiResponse<any>> {
    return this.api.put('/profile', data);
  }

  updatePreferences(data: any): Observable<ApiResponse<any>> {
    return this.api.put('/profile/preferences', data);
  }

  changePassword(data: any): Observable<ApiResponse<any>> {
    return this.api.put('/profile/password', data);
  }

  uploadPicture(file: File): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<any>>(`${this.base}/profile/picture`, formData);
  }

  updateSecurityQuestion(data: any): Observable<ApiResponse<any>> {
    return this.api.put('/profile/security-question', data);
  }

  updateTwoFactor(data: any): Observable<ApiResponse<any>> {
    return this.api.put('/profile/two-factor', data);
  }

  getSessions(): Observable<ApiResponse<any>> {
    return this.api.get('/profile/sessions');
  }

  revokeSession(id: string): Observable<ApiResponse<any>> {
    return this.api.delete(`/profile/sessions/${id}`);
  }

  getActivities(): Observable<ApiResponse<any>> {
    return this.api.get('/profile/activities');
  }
}