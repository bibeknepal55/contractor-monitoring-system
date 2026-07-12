import { Component, inject, OnInit, ViewChild, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule, MatSidenav } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService } from '../../core/services/language.service';
import { NotificationService } from '../../core/services/notification.service';
import { ApprovalService } from '../../core/services/approval.service';
import { UserProfile } from '../../core/models/api-response.model';
import { ROLE_COLORS } from '../../core/models/user.model';
import { Observable, map, shareReplay } from 'rxjs';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
  roles?: string[];
  children?: NavItem[];
  badge?: string;
  badgeColor?: string;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MatSidenavModule, MatToolbarModule, MatButtonModule, MatIconModule, MatListModule, MatMenuModule, MatDividerModule, MatTooltipModule, MatBadgeModule],
  templateUrl: './main-layout.component.html',
  styles: [`
    :host { display: block; height: 100vh; }
    .layout-container { display: flex; flex-direction: column; height: 100%; }
    .app-toolbar { position: fixed; top: 0; left: 0; right: 0; z-index: 100; height: 64px; background: linear-gradient(135deg, #0d47a1 0%, #1a73e8 100%); color: white; display: flex; align-items: center; padding: 0 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.15); }
    .toolbar-left { display: flex; align-items: center; gap: 12px; }
    .menu-button { color: white; margin-right: 4px; }
    .app-logo { display: flex; align-items: center; gap: 8px; cursor: pointer; }
    .app-logo mat-icon { font-size: 28px; width: 28px; height: 28px; color: #ffd54f; }
    .app-logo-text { font-size: 1.1rem; font-weight: 600; white-space: nowrap; }
    .toolbar-spacer { flex: 1; }
    .toolbar-right { display: flex; align-items: center; gap: 6px; }
    .notif-btn { color: white; margin-right: 4px; }
    .user-button { display: flex; align-items: center; gap: 10px; padding: 4px 14px 4px 6px; border-radius: 24px; background: rgba(255,255,255,0.12); border: 1px solid rgba(255,255,255,0.2); transition: background 0.2s; height: 42px; min-width: auto; line-height: normal; cursor: pointer; }
    .user-button:hover { background: rgba(255,255,255,0.22); }
    .user-avatar { width: 32px; height: 32px; min-width: 32px; border-radius: 50%; overflow: hidden; background: rgba(255,255,255,0.3); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .menu-avatar { width: 52px; height: 52px; border-radius: 50%; overflow: hidden; background: linear-gradient(135deg, #1a73e8, #64b5f6); display: flex; align-items: center; justify-content: center; margin: 0 auto 10px; }
    .sidebar-user-avatar { width: 56px; height: 56px; border-radius: 50%; overflow: hidden; background: linear-gradient(135deg, #1a73e8, #64b5f6); display: flex; align-items: center; justify-content: center; margin: 0 auto 8px; }
    .avatar-pic { width: 100%; height: 100%; object-fit: cover; }
    .avatar-text { font-weight: 600; font-size: 0.85rem; color: white; }
    .avatar-text-menu { font-weight: 700; font-size: 1.2rem; color: white; }
    .avatar-text-sidebar { font-weight: 700; font-size: 1.3rem; color: white; }
    .user-info { display: flex; flex-direction: column; line-height: 1.2; overflow: hidden; }
    .user-name { font-size: 0.82rem; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .user-role { font-size: 0.68rem; opacity: 0.8; }
    .dropdown-arrow { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; opacity: 0.7; margin-left: -2px; }
    .menu-header { padding: 16px; text-align: center; min-width: 220px; }
    .menu-user-name { font-weight: 600; font-size: 0.95rem; color: #212121; }
    .menu-user-email { font-size: 0.78rem; color: #757575; margin-bottom: 6px; }
    .role-badge-inline { display: inline-block; padding: 3px 12px; border-radius: 12px; font-size: 0.7rem; font-weight: 600; }
    .role-badge-inline.superadmin { background: #f3e5f5; color: #6a1b9a; }
    .role-badge-inline.admin { background: #e3f2fd; color: #1565c0; }
    .role-badge-inline.test { background: #e8f5e9; color: #2e7d32; }
    .role-badge-inline.viewer { background: #f5f5f5; color: #616161; }
    .sidenav-container { flex: 1; margin-top: 64px; height: calc(100vh - 64px); }
    .app-sidenav { width: 260px; background: white; border-right: 1px solid #e0e0e0; overflow-y: auto; }
    .sidebar-header { padding: 20px 16px 12px; text-align: center; }
    .sidebar-user-name { font-weight: 600; font-size: 0.95rem; color: #212121; }
    .sidebar-user-email { font-size: 0.75rem; color: #757575; margin-bottom: 8px; }
    .nav-section-label { padding: 16px 20px 6px; font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 1.2px; color: #9e9e9e; }
    .nav-list { padding: 0 8px; }
    .nav-item { display: flex; align-items: center; gap: 12px; padding: 10px 16px; margin: 2px 0; border-radius: 10px; color: #424242; text-decoration: none; transition: all 0.2s; cursor: pointer; font-size: 0.875rem; position: relative; }
    .nav-item:hover { background: #f5f5f5; color: #1a73e8; }
    .nav-item.active { background: #e3f2fd; color: #0d47a1; font-weight: 500; }
    .nav-item.active::before { content: ''; position: absolute; left: 0; top: 8px; bottom: 8px; width: 3px; background: #1a73e8; border-radius: 0 2px 2px 0; }
    .nav-item mat-icon { font-size: 22px; width: 22px; height: 22px; flex-shrink: 0; }
    .nav-item-label { flex: 1; }
    .nav-item-badge { font-size: 0.7rem; padding: 2px 8px; border-radius: 10px; font-weight: 600; }
    .nav-item-arrow { font-size: 18px; width: 18px; height: 18px; transition: transform 0.2s; }
    .nav-item-arrow.expanded { transform: rotate(180deg); }
    .nav-sub-items { padding-left: 16px; }
    .nav-sub-item { display: flex; align-items: center; gap: 8px; padding: 8px 16px; margin: 1px 0; border-radius: 8px; color: #616161; text-decoration: none; transition: all 0.2s; font-size: 0.8rem; }
    .nav-sub-item:hover { background: #f5f5f5; color: #1a73e8; }
    .nav-sub-item.active { background: #e8eaf6; color: #1a73e8; font-weight: 500; }

    /* DISABLED STYLES FOR VIEWER */
    .nav-item.disabled { color: #bdbdbd !important; cursor: not-allowed !important; pointer-events: none !important; opacity: 0.5; background: transparent !important; }
    .nav-item.disabled:hover { background: transparent !important; color: #bdbdbd !important; }
    .nav-item.disabled mat-icon { color: #bdbdbd !important; opacity: 0.5; }
    .nav-sub-item.disabled { color: #bdbdbd !important; cursor: not-allowed !important; pointer-events: none !important; opacity: 0.5; }
    .nav-sub-item.disabled:hover { background: transparent !important; color: #bdbdbd !important; }

    .sidebar-footer { padding: 16px; border-top: 1px solid #e0e0e0; margin-top: auto; }
    .main-content { background: #f5f7fa; min-height: 100%; }
    @media (max-width: 768px) { .app-logo-text { display: none; } .user-name, .user-role, .user-info { display: none; } .user-button { padding: 4px 10px 4px 4px; } .dropdown-arrow { margin-left: 0; } }
  `]
})
export class MainLayoutComponent implements OnInit {
  private readonly authService = inject(AuthService);
  readonly langSrv = inject(LanguageService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly approvalService = inject(ApprovalService);
  private readonly zone = inject(NgZone);

  @ViewChild('sidenav') sidenav!: MatSidenav;

  currentUser: UserProfile | null = null;
  userInitials: string = '';
  roleColor: string = '#757575';
  pendingApprovals: number = 0;
  profilePictureUrl: string = '';
  isViewerOnly = false;

  isHandset$: Observable<boolean> = this.breakpointObserver.observe([Breakpoints.Handset, Breakpoints.TabletPortrait]).pipe(map((result) => result.matches), shareReplay(1));
  expandedGroups: Set<string> = new Set(['Business Modules']);

  allNavItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard', permission: 'Dashboard.View' },
    { label: 'Projects', icon: 'business', route: '/projects', permission: 'Project.View' },
    { label: 'Contractors', icon: 'groups', route: '/contractors', permission: 'ContractorOfficeDetail.View' },
    {
      label: 'Business Modules', icon: 'folder', route: '',
      children: [
        { label: 'Contract Financials', icon: 'account_balance', route: '/financials', permission: 'ContractFinancialDetail.View' },
        { label: 'Price Adjustments', icon: 'trending_up', route: '/price-adjustments', permission: 'PriceAdjustment.View' },
        { label: 'Performance Bonds', icon: 'verified', route: '/bonds', permission: 'PerformanceBond.View' },
        { label: 'Advance Payment Guarantees', icon: 'shield', route: '/guarantees', permission: 'AdvancePaymentGuarantee.View' },
        { label: 'Physical Progress', icon: 'trending_up', route: '/progress', permission: 'PhysicalProgress.View' },
        { label: 'Time Extensions', icon: 'schedule', route: '/time-extensions', permission: 'TimeExtension.View' },
        { label: 'Delay Reasons', icon: 'warning', route: '/delays', permission: 'DelayReason.View' },
        { label: 'Raw Materials', icon: 'inventory', route: '/materials', permission: 'RawMaterial.View' },
        { label: 'Lab Tests', icon: 'science', route: '/lab-tests', permission: 'LabTest.View' },
        { label: 'Photo Monitoring', icon: 'photo_camera', route: '/photos', permission: 'PhotoMonitoring.View' },
        { label: 'Subcontractors', icon: 'handshake', route: '/subcontractors', permission: 'Subcontractor.View' },
        { label: 'Responsible Officials', icon: 'badge', route: '/officials', permission: 'ResponsibleOfficial.View' },
      ],
    },
    { label: 'Approval Workflow', icon: 'fact_check', route: '/approvals', permission: 'ApprovalWorkflow.View', roles: ['SuperAdmin', 'Admin', 'Test'], badge: '0', badgeColor: '#f57c00' },
    { label: 'Reports', icon: 'assessment', route: '/reports', permission: 'Reports.View', roles: ['SuperAdmin', 'Admin', 'Test'] },
  ];

  adminNavItems: NavItem[] = [
    { label: 'User Management', icon: 'people', route: '/users', permission: 'UserManagement.View', roles: ['SuperAdmin', 'Admin'] },
    { label: 'Role Management', icon: 'admin_panel_settings', route: '/roles', permission: 'UserManagement.View', roles: ['SuperAdmin'] },
    {
      label: 'User Activity',
      icon: 'history',
      route: '',
      roles: ['SuperAdmin', 'Admin'],
      children: [
        { label: 'User Logs', icon: 'list_alt', route: '/user-logs', permission: 'UserManagement.View' },
      ]
    },
  ];

  // ========== USER-SPECIFIC PROFILE PICTURE KEY ==========
  private get userPictureKey(): string {
    const userId = this.currentUser?.id || 'anonymous';
    return `profilePicture_${userId}`;
  }

  get visibleNavItems(): NavItem[] {
    return [...this.allNavItems.filter(i => this.canShowNavItem(i)), ...this.adminNavItems.filter(i => this.canShowNavItem(i))];
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (this.currentUser) {
      this.userInitials = `${this.currentUser.firstName.charAt(0)}${this.currentUser.lastName.charAt(0)}`.toUpperCase();
      this.roleColor = ROLE_COLORS[this.authService.getHighestRole() || ''] || '#757575';
    }
    const roles = this.currentUser?.roles || [];
    this.isViewerOnly = roles.length === 1 && roles[0] === 'Viewer';

    this.refreshProfilePicture();
    this.zone.runOutsideAngular(() => {
      setInterval(() => {
        // Use user-specific key to get the correct picture for the logged-in user
        const savedPic = localStorage.getItem(this.userPictureKey);
        if (savedPic && savedPic !== 'undefined' && savedPic !== 'null' && savedPic.length > 100) {
          if (savedPic !== this.profilePictureUrl) {
            this.zone.run(() => { this.profilePictureUrl = savedPic; });
          }
        } else {
          // If no picture for this user, clear the URL
          if (this.profilePictureUrl) {
            this.zone.run(() => { this.profilePictureUrl = ''; });
          }
        }
      }, 2000);
    });
    this.isHandset$.subscribe(isHandset => { if (this.sidenav) { this.sidenav.mode = isHandset ? 'over' : 'side'; isHandset ? this.sidenav.close() : this.sidenav.open(); } });
    this.loadPendingCount();
  }

  refreshProfilePicture(): void {
    // Use user-specific key
    const savedPic = localStorage.getItem(this.userPictureKey);
    if (savedPic && savedPic !== 'undefined' && savedPic !== 'null' && savedPic.length > 100) {
      this.profilePictureUrl = savedPic;
    } else {
      this.profilePictureUrl = '';
    }
  }

  loadPendingCount(): void {
    if (!this.authService.hasPermission('ApprovalWorkflow.View') || this.isViewerOnly) return;
    this.approvalService.getAll({ page: 1, pageSize: 1, status: 'Pending' } as any).subscribe({
      next: (r: any) => { if (r.success) { this.pendingApprovals = r.totalCount || 0; const item = this.allNavItems.find(i => i.label === 'Approval Workflow'); if (item) item.badge = String(this.pendingApprovals); } },
      error: () => {}
    });
  }

  hasPermission(p: string): boolean { return this.authService.hasPermission(p); }

  canShowNavItem(item: NavItem): boolean {
    if (item.roles?.length && !this.authService.hasAnyRole(item.roles)) return false;
    if (item.permission && !this.authService.hasPermission(item.permission)) return false;
    if (item.children) return item.children.some(c => this.canShowNavItem(c));
    return true;
  }

  isNavDisabled(item: NavItem): boolean {
    if (!this.isViewerOnly) return false;
    if (item.route === '/dashboard') return false;
    return true;
  }

  toggleGroup(g: string): void {
    if (this.isViewerOnly) return;
    this.expandedGroups.has(g) ? this.expandedGroups.delete(g) : this.expandedGroups.add(g);
  }

  isGroupExpanded(g: string): boolean { return this.expandedGroups.has(g); }

  closeSidenavOnMobile(): void {
    const sub = this.isHandset$.subscribe(h => { if (h && this.sidenav) this.sidenav.close(); });
    sub.unsubscribe();
  }

  changePassword(): void { this.router.navigate(['/change-password']); }
  getHighestRole(): string { return this.authService.getHighestRole() || 'Viewer'; }

  logout(): void {
    this.notificationService.confirmAction(
      this.langSrv.t('Logout'),
      this.langSrv.getLang() === 'ne' ? 'के तपाईं लगआउट गर्न निश्चित हुनुहुन्छ?' : 'Are you sure you want to logout?',
      this.langSrv.t('Logout')
    ).then(confirmed => {
      if (confirmed) {
        this.authService.logout().subscribe({
          next: () => { this.authService.clearSession(); this.router.navigate(['/auth/login']); },
          error: () => { this.authService.clearSession(); this.router.navigate(['/auth/login']); }
        });
      }
    });
  }
}