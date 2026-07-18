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

  // Tokens stored in MEMORY only - NOT localStorage (prevents XSS token theft)
  private accessToken: string | null = null;
  private refreshTokenStr: string | null = null;
  private expiresAtStr: string | null = null;

  private readonly currentUserSubject = new BehaviorSubject<UserProfile | null>(this.getUserFromStorage());
  public readonly currentUser$ = this.currentUserSubject.asObservable();

  // Only user profile is in localStorage (non-sensitive display data)
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
    const token = this.accessToken;
    this.clearSession();
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/auth/logout`, {});
  }

  setSession(authResponse: AuthResponse): void {
    // Store tokens in MEMORY only - prevents XSS theft
    this.accessToken = authResponse.accessToken;
    this.refreshTokenStr = authResponse.refreshToken;
    this.expiresAtStr = authResponse.expiresAt;

    // Only store non-sensitive user info in localStorage for UI display
    const userForStorage = {
      id: authResponse.user.id,
      firstName: authResponse.user.firstName,
      lastName: authResponse.user.lastName,
      email: authResponse.user.email,
      roles: authResponse.user.roles,
      permissions: authResponse.user.permissions,
    };
    localStorage.setItem('currentUser', JSON.stringify(userForStorage));
    this.currentUserSubject.next(authResponse.user);
  }

  clearSession(): void {
    this.accessToken = null;
    this.refreshTokenStr = null;
    this.expiresAtStr = null;
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  getRefreshToken(): string | null {
    return this.refreshTokenStr;
  }

  getExpiresAt(): Date | null {
    return this.expiresAtStr ? new Date(this.expiresAtStr) : null;
  }

  getCurrentUser(): UserProfile | null {
    return this.currentUserSubject.value;
  }

  // Force refresh user permissions from server
  refreshUserPermissions(): void {
    const user = this.getCurrentUser();
    if (!user) return;
    
    this.http.get<ApiResponse<any>>(`${this.baseUrl}/profile`).subscribe({
      next: (r: any) => {
        if (r.success && r.data) {
          const updatedUser = {
            ...user,
            roles: r.data.roles || user.roles,
            permissions: r.data.permissions || user.permissions,
          };
          const userForStorage = {
            id: updatedUser.id,
            firstName: updatedUser.firstName,
            lastName: updatedUser.lastName,
            email: updatedUser.email,
            roles: updatedUser.roles,
            permissions: updatedUser.permissions,
          };
          localStorage.setItem('currentUser', JSON.stringify(userForStorage));
          this.currentUserSubject.next(updatedUser);
        }
      },
      error: () => {}
    });
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