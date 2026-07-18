import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PhotoService {
  private api = inject(ApiService);
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private ep = '/photo-monitoring';

  getAll(req: PagedRequest): Observable<PagedResponse<any>> { return this.api.getPaged(this.ep, req); }
  getById(id: string): Observable<ApiResponse<any>> { return this.api.get(`${this.ep}/${id}`); }
  create(d: any): Observable<ApiResponse<any>> { return this.api.post(this.ep, d); }
  update(id: string, d: any): Observable<ApiResponse<any>> { return this.api.put(`${this.ep}/${id}`, d); }
  delete(id: string): Observable<ApiResponse<boolean>> { return this.api.delete(`${this.ep}/${id}`); }
  upload(formData: FormData): Observable<ApiResponse<any>> { return this.api.upload(`${this.ep}/upload`, formData); }

  // Upload photo with file + form fields
  createWithFile(formData: FormData): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}${this.ep}/upload`, formData);
  }

  updateWithFile(id: string, formData: FormData): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}${this.ep}/upload/${id}`, formData);
  }
}