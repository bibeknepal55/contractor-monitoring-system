import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';
import { Contractor } from '../models/contractor.model';

@Injectable({
  providedIn: 'root',
})
export class ContractorService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = '/contractors';

  getContractors(request: PagedRequest): Observable<PagedResponse<Contractor>> {
    return this.apiService.getPaged<Contractor>(this.endpoint, request);
  }

  getContractorById(id: string): Observable<ApiResponse<Contractor>> {
    return this.apiService.get<Contractor>(`${this.endpoint}/${id}`);
  }

  createContractor(contractor: any): Observable<ApiResponse<Contractor>> {
    return this.apiService.post<Contractor>(this.endpoint, contractor);
  }

  updateContractor(id: string, contractor: any): Observable<ApiResponse<Contractor>> {
    return this.apiService.put<Contractor>(`${this.endpoint}/${id}`, contractor);
  }

  deleteContractor(id: string): Observable<ApiResponse<boolean>> {
    return this.apiService.delete<boolean>(`${this.endpoint}/${id}`);
  }
}