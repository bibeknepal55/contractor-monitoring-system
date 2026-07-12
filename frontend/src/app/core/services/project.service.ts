import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse, PagedRequest } from '../models/api-response.model';
import { Project } from '../models/project.model';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = '/projects';

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
}