import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';
import { ResourceStore } from './resource-store';

export interface Project {
  id: string;
  projectCode: string;
  projectName: string;
  description?: string;
  location?: string;
  status?: string;
  priority?: string;
  budget?: number;
  startDate?: string;
  endDate?: string;
  contractorId?: string;
  contractorName?: string;
  projectManager?: string;
  contactNumber?: string;
  contractNumber?: string;
  isActive?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = '/projects';

  private readonly store = new ResourceStore<Project>();
  public readonly projects$ = this.store.state$;
  public get currentProjects() { return this.store.current; }

  getProjects(request: PagedRequest): Observable<PagedResponse<Project>> {
    return this.apiService.getPaged<Project>(this.endpoint, request);
  }

  getProjectById(id: string): Observable<ApiResponse<Project>> {
    return this.apiService.get<Project>(`${this.endpoint}/${id}`);
  }

  createProject(project: any): Observable<ApiResponse<Project>> {
    return this.apiService.post<Project>(this.endpoint, project);
  }

  updateProject(id: string, project: any): Observable<ApiResponse<Project>> {
    return this.apiService.put<Project>(`${this.endpoint}/${id}`, project);
  }

  deleteProject(id: string): Observable<ApiResponse<boolean>> {
    return this.apiService.delete<boolean>(`${this.endpoint}/${id}`);
  }

  loadProjects(params: PagedRequest): void {
    this.store.setLoading(true);
    this.getProjects(params).subscribe({
      next: (response: PagedResponse<Project>) => {
        if (response.success) {
          this.store.setData(response.data, response.totalCount || response.data.length);
        } else {
          this.store.setError(response.message || 'Failed to load projects');
        }
      },
      error: (error: any) => {
        this.store.setError(error?.error?.message || 'Failed to load projects. Please try again.');
      }
    });
  }

  loadProjectById(id: string): void {
    this.store.setLoading(true);
    this.getProjectById(id).subscribe({
      next: (response: ApiResponse<Project>) => {
        if (response.success && response.data) {
          this.store.setData([response.data], 1);
        } else {
          this.store.setError(response.message || 'Project not found');
        }
      },
      error: (error: any) => {
        this.store.setError(error?.error?.message || 'Failed to load project');
      }
    });
  }

  createAndRefresh(project: any): Observable<ApiResponse<Project>> {
    const observable = this.createProject(project);
    observable.subscribe({
      next: (response: any) => {
        if (response.success) {
          this.loadProjects({ page: 1, pageSize: this.store.current.data.length || 10 });
        }
      }
    });
    return observable;
  }

  updateAndRefresh(id: string, project: any): Observable<ApiResponse<Project>> {
    const observable = this.updateProject(id, project);
    observable.subscribe({
      next: (response: any) => {
        if (response.success) {
          this.loadProjects({ page: 1, pageSize: this.store.current.data.length || 10 });
        }
      }
    });
    return observable;
  }

  deleteAndRefresh(id: string): Observable<ApiResponse<boolean>> {
    const observable = this.deleteProject(id);
    observable.subscribe({
      next: (response: any) => {
        if (response.success) {
          this.loadProjects({ page: 1, pageSize: this.store.current.data.length || 10 });
        }
      }
    });
    return observable;
  }

  clearStore(): void {
    this.store.clear();
  }

  refresh(): void {
    this.loadProjects({ page: 1, pageSize: this.store.current.data.length || 10 });
  }
}