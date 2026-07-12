import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../../environments/environment';
import { ApiResponse, LoginRequest, RegisterRequest, ChangePasswordRequest, RefreshTokenRequest, AuthResponse, UserProfile } from '../models/api-response.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly jwtHelper = inject(JwtHelperService);
  private readonly baseUrl = environment.apiUrl;

  private readonly currentUserSubject = new BehaviorSubject<UserProfile | null>(this.getUserFromStorage());
  public readonly currentUser$ = this.currentUserSubject.asObservable();

  private getUserFromStorage(): UserProfile | null {
    const userJson = localStorage.getItem('currentUser');
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch {
        return null;
      }
    }
    return null;
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;
    try {
      return !this.jwtHelper.isTokenExpired(token);
    } catch {
      return false;
    }
  }

  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/login`, request);
  }

  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/register`, request);
  }

  refreshToken(request: RefreshTokenRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/refresh-token`, request);
  }

  changePassword(request: ChangePasswordRequest): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/auth/change-password`, request);
  }

  logout(): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/auth/logout`, {});
  }

  setSession(authResponse: AuthResponse): void {
    localStorage.setItem('accessToken', authResponse.accessToken);
    localStorage.setItem('refreshToken', authResponse.refreshToken);
    localStorage.setItem('expiresAt', authResponse.expiresAt);
    localStorage.setItem('currentUser', JSON.stringify(authResponse.user));
    this.currentUserSubject.next(authResponse.user);
  }

  clearSession(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expiresAt');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  getExpiresAt(): Date | null {
    const expiresAt = localStorage.getItem('expiresAt');
    return expiresAt ? new Date(expiresAt) : null;
  }

  getCurrentUser(): UserProfile | null {
    return this.currentUserSubject.value;
  }

  hasPermission(permission: string): boolean {
    const user = this.getCurrentUser();
    if (!user || !user.permissions) return false;
    if (user.roles.includes('SuperAdmin')) return true;
    return user.permissions.includes(permission);
  }

  hasAnyPermission(permissions: string[]): boolean {
    return permissions.some((p) => this.hasPermission(p));
  }

  hasAllPermissions(permissions: string[]): boolean {
    return permissions.every((p) => this.hasPermission(p));
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    if (!user || !user.roles) return false;
    return user.roles.includes(role);
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some((r) => this.hasRole(r));
  }

  getHighestRole(): string | null {
    const user = this.getCurrentUser();
    if (!user || !user.roles) return null;
    if (user.roles.includes('SuperAdmin')) return 'SuperAdmin';
    if (user.roles.includes('Admin')) return 'Admin';
    if (user.roles.includes('Test')) return 'Test';
    if (user.roles.includes('Viewer')) return 'Viewer';
    return null;
  }

  isTokenExpired(): boolean {
    const token = this.getAccessToken();
    if (!token) return true;
    try {
      return this.jwtHelper.isTokenExpired(token);
    } catch {
      return true;
    }
  }

  shouldRefreshToken(): boolean {
    const expiresAt = this.getExpiresAt();
    if (!expiresAt) return false;
    const thresholdMinutes = environment.tokenRefreshThresholdMinutes;
    const threshold = new Date(Date.now() + thresholdMinutes * 60 * 1000);
    return expiresAt <= threshold;
  }

  getAvailableRolesForAssignment(): string[] {
    const highestRole = this.getHighestRole();
    switch (highestRole) {
      case 'SuperAdmin':
        return ['SuperAdmin', 'Admin', 'Test', 'Viewer'];
      case 'Admin':
        return ['Test', 'Viewer'];
      case 'Test':
        return ['Test'];
      default:
        return [];
    }
  }

  canManageUser(targetUserRoles: string[]): boolean {
    const highestRole = this.getHighestRole();
    if (highestRole === 'SuperAdmin') return true;
    if (highestRole === 'Admin') {
      return !targetUserRoles.includes('SuperAdmin') && !targetUserRoles.includes('Admin');
    }
    if (highestRole === 'Test') {
      return targetUserRoles.every((r) => r === 'Test' || r === 'Viewer');
    }
    return false;
  }

  canDeleteUser(targetUserRoles: string[]): boolean {
    const highestRole = this.getHighestRole();
    if (highestRole === 'SuperAdmin') return true;
    return false;
  }
}