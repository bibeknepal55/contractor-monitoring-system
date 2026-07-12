import { PagedRequest } from './api-response.model';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  isActive: boolean;
  roles: Role[];
  permissions: string[];
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface Role {
  id: string;
  name: string;
  description?: string;
  permissions: string[];
  userCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface AssignRoleRequest {
  roles: string[];
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}

export interface UpdateRolePermissionsRequest {
  permissions: string[];
}

export interface UserListRequest extends PagedRequest {
  role?: string;
  isActive?: boolean;
}

export enum UserRole {
  SuperAdmin = 'SuperAdmin',
  Admin = 'Admin',
  Test = 'Test',
  Viewer = 'Viewer',
}

export const USER_ROLES = [
  { value: UserRole.SuperAdmin, label: 'Super Admin' },
  { value: UserRole.Admin, label: 'Admin' },
  { value: UserRole.Test, label: 'Test' },
  { value: UserRole.Viewer, label: 'Viewer' },
];

export const ROLE_COLORS: Record<string, string> = {
  [UserRole.SuperAdmin]: '#9c27b0',
  [UserRole.Admin]: '#1976d2',
  [UserRole.Test]: '#388e3c',
  [UserRole.Viewer]: '#757575',
};