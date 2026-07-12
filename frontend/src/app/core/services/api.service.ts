import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, finalize, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly spinner = inject(NgxSpinnerService);
  private readonly baseUrl = environment.apiUrl;
  private activeRequests = 0;

  private showLoader(): void {
    this.activeRequests++;
    if (this.activeRequests === 1) {
      this.spinner.show();
    }
  }

  private hideLoader(): void {
    this.activeRequests--;
    if (this.activeRequests <= 0) {
      this.activeRequests = 0;
      this.spinner.hide();
    }
  }

  get<T>(endpoint: string, params?: Record<string, string | number | boolean | undefined>): Observable<ApiResponse<T>> {
    this.showLoader();
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          httpParams = httpParams.set(key, String(value));
        }
      });
    }
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, { params: httpParams })
      .pipe(finalize(() => this.hideLoader()));
  }

  getPaged<T>(endpoint: string, request: PagedRequest): Observable<PagedResponse<T>> {
    this.showLoader();
    let httpParams = new HttpParams()
      .set('page', request.page.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.search) httpParams = httpParams.set('search', request.search);
    if (request.sortBy) httpParams = httpParams.set('sortBy', request.sortBy);
    if (request.sortOrder) httpParams = httpParams.set('sortOrder', request.sortOrder);

    return this.http.get<PagedResponse<T>>(`${this.baseUrl}${endpoint}`, { params: httpParams })
      .pipe(finalize(() => this.hideLoader()));
  }

  post<T>(endpoint: string, body: unknown): Observable<ApiResponse<T>> {
    this.showLoader();
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, body)
      .pipe(finalize(() => this.hideLoader()));
  }

  put<T>(endpoint: string, body: unknown): Observable<ApiResponse<T>> {
    this.showLoader();
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, body)
      .pipe(finalize(() => this.hideLoader()));
  }

  delete<T>(endpoint: string): Observable<ApiResponse<T>> {
    this.showLoader();
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}${endpoint}`)
      .pipe(finalize(() => this.hideLoader()));
  }

  upload<T>(endpoint: string, formData: FormData): Observable<ApiResponse<T>> {
    this.showLoader();
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}${endpoint}`, formData)
      .pipe(finalize(() => this.hideLoader()));
  }

  download(endpoint: string, params?: Record<string, string | number | boolean | undefined>): Observable<Blob> {
    this.showLoader();
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          httpParams = httpParams.set(key, String(value));
        }
      });
    }
    return this.http.get(`${this.baseUrl}${endpoint}`, {
      params: httpParams,
      responseType: 'blob',
    }).pipe(finalize(() => this.hideLoader()));
  }

  postDownload<T>(endpoint: string, body: unknown): Observable<Blob> {
    this.showLoader();
    return this.http.post(`${this.baseUrl}${endpoint}`, body, {
      responseType: 'blob',
    }).pipe(finalize(() => this.hideLoader()));
  }
}