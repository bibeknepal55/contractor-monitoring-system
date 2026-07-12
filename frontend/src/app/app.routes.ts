import { Routes } from '@angular/router';
import { authGuard, noAuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    canActivate: [noAuthGuard],
    children: [
      { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
      { path: '', redirectTo: 'login', pathMatch: 'full' },
    ],
  },
  {
    path: 'change-password',
    loadComponent: () => import('./features/auth/change-password/change-password.component').then(m => m.ChangePasswordComponent),
    canActivate: [authGuard],
  },
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      
      // Projects
      { path: 'projects', loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent) },
      { path: 'projects/new', loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent) },
      { path: 'projects/:id/edit', loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent) },
      
      // Contractors
      { path: 'contractors', loadComponent: () => import('./features/contractors/contractor-list/contractor-list.component').then(m => m.ContractorListComponent) },
      { path: 'contractors/new', loadComponent: () => import('./features/contractors/contractor-form/contractor-form.component').then(m => m.ContractorFormComponent) },
      { path: 'contractors/:id/edit', loadComponent: () => import('./features/contractors/contractor-form/contractor-form.component').then(m => m.ContractorFormComponent) },
      
      // Financials
      { path: 'financials', loadComponent: () => import('./features/financials/financial-list/financial-list.component').then(m => m.FinancialListComponent) },
      { path: 'financials/new', loadComponent: () => import('./features/financials/financial-form/financial-form.component').then(m => m.FinancialFormComponent) },
      { path: 'financials/:id/edit', loadComponent: () => import('./features/financials/financial-form/financial-form.component').then(m => m.FinancialFormComponent) },
      
      // Price Adjustments
      { path: 'price-adjustments', loadComponent: () => import('./features/price-adjustments/price-adjustment-list/price-adjustment-list.component').then(m => m.PriceAdjustmentListComponent) },
      { path: 'price-adjustments/new', loadComponent: () => import('./features/price-adjustments/price-adjustment-form/price-adjustment-form.component').then(m => m.PriceAdjustmentFormComponent) },
      { path: 'price-adjustments/:id/edit', loadComponent: () => import('./features/price-adjustments/price-adjustment-form/price-adjustment-form.component').then(m => m.PriceAdjustmentFormComponent) },
      
      // Bonds
      { path: 'bonds', loadComponent: () => import('./features/bonds/bond-list/bond-list.component').then(m => m.BondListComponent) },
      { path: 'bonds/new', loadComponent: () => import('./features/bonds/bond-form/bond-form.component').then(m => m.BondFormComponent) },
      { path: 'bonds/:id/edit', loadComponent: () => import('./features/bonds/bond-form/bond-form.component').then(m => m.BondFormComponent) },
      
      // Guarantees
      { path: 'guarantees', loadComponent: () => import('./features/guarantees/guarantee-list/guarantee-list.component').then(m => m.GuaranteeListComponent) },
      { path: 'guarantees/new', loadComponent: () => import('./features/guarantees/guarantee-form/guarantee-form.component').then(m => m.GuaranteeFormComponent) },
      { path: 'guarantees/:id/edit', loadComponent: () => import('./features/guarantees/guarantee-form/guarantee-form.component').then(m => m.GuaranteeFormComponent) },
      
      // Progress
      { path: 'progress', loadComponent: () => import('./features/progress/progress-list/progress-list.component').then(m => m.ProgressListComponent) },
      { path: 'progress/new', loadComponent: () => import('./features/progress/progress-form/progress-form.component').then(m => m.ProgressFormComponent) },
      { path: 'progress/:id/edit', loadComponent: () => import('./features/progress/progress-form/progress-form.component').then(m => m.ProgressFormComponent) },
      
      // Time Extensions
      { path: 'time-extensions', loadComponent: () => import('./features/time-extensions/time-extension-list/time-extension-list.component').then(m => m.TimeExtensionListComponent) },
      { path: 'time-extensions/new', loadComponent: () => import('./features/time-extensions/time-extension-form/time-extension-form.component').then(m => m.TimeExtensionFormComponent) },
      { path: 'time-extensions/:id/edit', loadComponent: () => import('./features/time-extensions/time-extension-form/time-extension-form.component').then(m => m.TimeExtensionFormComponent) },
      
      // Delays
      { path: 'delays', loadComponent: () => import('./features/delays/delay-list/delay-list.component').then(m => m.DelayListComponent) },
      { path: 'delays/new', loadComponent: () => import('./features/delays/delay-form/delay-form.component').then(m => m.DelayFormComponent) },
      { path: 'delays/:id/edit', loadComponent: () => import('./features/delays/delay-form/delay-form.component').then(m => m.DelayFormComponent) },
      
      // Materials
      { path: 'materials', loadComponent: () => import('./features/materials/material-list/material-list.component').then(m => m.MaterialListComponent) },
      { path: 'materials/new', loadComponent: () => import('./features/materials/material-form/material-form.component').then(m => m.MaterialFormComponent) },
      { path: 'materials/:id/edit', loadComponent: () => import('./features/materials/material-form/material-form.component').then(m => m.MaterialFormComponent) },
      
      // Lab Tests
      { path: 'lab-tests', loadComponent: () => import('./features/lab-tests/lab-test-list/lab-test-list.component').then(m => m.LabTestListComponent) },
      { path: 'lab-tests/new', loadComponent: () => import('./features/lab-tests/lab-test-form/lab-test-form.component').then(m => m.LabTestFormComponent) },
      { path: 'lab-tests/:id/edit', loadComponent: () => import('./features/lab-tests/lab-test-form/lab-test-form.component').then(m => m.LabTestFormComponent) },
      
      //sub contractors and officials

      { path: 'subcontractors', loadComponent: () => import('./features/subcontractors/subcontractor-list/subcontractor-list.component').then(m => m.SubcontractorListComponent) },
{ path: 'subcontractors/new', loadComponent: () => import('./features/subcontractors/subcontractor-form/subcontractor-form.component').then(m => m.SubcontractorFormComponent) },
{ path: 'subcontractors/:id/edit', loadComponent: () => import('./features/subcontractors/subcontractor-form/subcontractor-form.component').then(m => m.SubcontractorFormComponent) },
{ path: 'officials', loadComponent: () => import('./features/officials/official-list/official-list.component').then(m => m.OfficialListComponent) },
{ path: 'officials/new', loadComponent: () => import('./features/officials/official-form/official-form.component').then(m => m.OfficialFormComponent) },
{ path: 'officials/:id/edit', loadComponent: () => import('./features/officials/official-form/official-form.component').then(m => m.OfficialFormComponent) },

    // approvals and reports 

 { path: 'approvals', loadComponent: () => import('./features/approvals/approval-list.component').then(m => m.ApprovalListComponent) },
{ path: 'reports', loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent) },

// users and roles 

{ path: 'users', loadComponent: () => import('./features/user-management/user-list/user-list.component').then(m => m.UserListComponent) },
{ path: 'roles', loadComponent: () => import('./features/user-management/role-management/role-management.component').then(m => m.RoleManagementComponent) },

// profile

{ path: 'profile', loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent) },

// User-Activity
{ path: 'user-logs', loadComponent: () => import('./features/user-activity/user-log-list/user-log-list.component').then(m => m.UserLogListComponent) },


// Photos
      { path: 'photos', loadComponent: () => import('./features/photos/photo-list/photo-list.component').then(m => m.PhotoListComponent) },
      { path: 'photos/new', loadComponent: () => import('./features/photos/photo-form/photo-form.component').then(m => m.PhotoFormComponent) },
      { path: 'photos/:id/edit', loadComponent: () => import('./features/photos/photo-form/photo-form.component').then(m => m.PhotoFormComponent) },
      
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: 'auth/login' },
];