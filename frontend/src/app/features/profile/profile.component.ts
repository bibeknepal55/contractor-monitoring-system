import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ProfileService } from '../../core/services/profile.service';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService } from '../../core/services/language.service';
import { NotificationService } from '../../core/services/notification.service';
import { environment } from '../../../environments/environment';
import moment from 'moment-timezone';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, MatButtonModule, MatIconModule,
    MatCardModule, MatTabsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatSlideToggleModule, MatProgressBarModule, MatDividerModule, MatTooltipModule,
    MatChipsModule, MatSnackBarModule
  ],
  template: `
    <div class="page">
      <mat-progress-bar *ngIf="loading" mode="indeterminate" color="primary" class="top-bar-loader"></mat-progress-bar>

      <div class="header-card">
        <div class="cover"></div>
        <div class="header-body">
          <div class="avatar-zone" (click)="fileInput.click()" matTooltip="Click to change photo">
            <img [src]="profilePictureUrl" *ngIf="profilePictureUrl" class="avatar-img" alt="Profile">
            <div class="avatar-placeholder" *ngIf="!profilePictureUrl">{{ getInitials() }}</div>
            <div class="avatar-hover"><mat-icon>camera_alt</mat-icon><span>{{ t('Change') }}</span></div>
          </div>
          <input #fileInput type="file" accept="image/*" (change)="onFileSelected($event)" style="display:none">

          <div class="header-info">
            <h1>{{ profile?.firstName || '...' }} {{ profile?.lastName || '' }}</h1>
            <p class="subtitle">{{ profile?.jobTitle || t('No job title') }}{{ profile?.department ? ' • ' + profile.department : '' }}{{ profile?.company ? ' • ' + profile.company : '' }}</p>
            <div class="badges">
              <span class="badge" [ngClass]="getHighestRole().toLowerCase()">{{ getHighestRole() }}</span>
              <span class="badge-status" [class.active]="profile?.isActive">{{ profile?.isActive ? t('Active') : t('Inactive') }}</span>
            </div>
            <div class="meta">
              <span><mat-icon>email</mat-icon> {{ profile?.email }}</span>
              <span><mat-icon>phone</mat-icon> {{ profile?.phoneNumber || t('Not set') }}</span>
            </div>
          </div>
        </div>
      </div>

      <mat-card class="tabs-card">
        <mat-tab-group animationDuration="200ms" (selectedTabChange)="onTabChange($event)" [selectedIndex]="selectedTab">
          
          <mat-tab [label]="t('Personal Info')">
            <div class="tab-inner">
              <form [formGroup]="profileForm" (ngSubmit)="saveProfile()">
                <div class="grid-2">
                  <mat-form-field appearance="outline"><mat-label>{{ t('First Name') }}</mat-label><input matInput formControlName="firstName"></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Last Name') }}</mat-label><input matInput formControlName="lastName"></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Phone Number') }}</mat-label><input matInput formControlName="phoneNumber"></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Job Title') }}</mat-label><input matInput formControlName="jobTitle"></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Department') }}</mat-label><input matInput formControlName="department"></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Company') }}</mat-label><input matInput formControlName="company"></mat-form-field>
                </div>
                <mat-form-field appearance="outline"><mat-label>{{ t('Bio') }}</mat-label><textarea matInput formControlName="bio" rows="3" [placeholder]="t('Tell us about yourself...')"></textarea></mat-form-field>
                <div class="form-actions">
                  <button mat-flat-button color="primary" type="submit" [disabled]="savingProfile || !profileForm.dirty">
                    <mat-icon>save</mat-icon> {{ t('Save Changes') }}
                  </button>
                </div>
              </form>
            </div>
          </mat-tab>

          <mat-tab [label]="t('Preferences')">
            <div class="tab-inner">
              <form [formGroup]="prefsForm" (ngSubmit)="savePreferences()">
                <h3 class="section-title"><mat-icon>language</mat-icon> {{ t('Regional Settings') }}</h3>
                <div class="grid-2">
                  <mat-form-field appearance="outline"><mat-label>{{ t('Timezone') }}</mat-label><mat-select formControlName="timezone">
                    <mat-option value="Asia/Kathmandu">Asia/Kathmandu (GMT+5:45)</mat-option>
                    <mat-option value="Asia/Kolkata">Asia/Kolkata (GMT+5:30)</mat-option>
                    <mat-option value="Asia/Dubai">Asia/Dubai (GMT+4)</mat-option>
                    <mat-option value="Europe/London">Europe/London (GMT)</mat-option>
                    <mat-option value="America/New_York">America/New York (GMT-5)</mat-option>
                  </mat-select></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Language') }}</mat-label><mat-select formControlName="language">
                    <mat-option value="en">English</mat-option>
                    <mat-option value="ne">Nepali (नेपाली)</mat-option>
                  </mat-select></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Theme') }}</mat-label><mat-select formControlName="theme">
                    <mat-option value="light">☀️ {{ t('Light') }}</mat-option>
                    <mat-option value="dark">🌙 {{ t('Dark') }}</mat-option>
                  </mat-select></mat-form-field>
                </div>

                <h3 class="section-title"><mat-icon>notifications</mat-icon> {{ t('Notification Preferences') }}</h3>
                <div class="toggle-list">
                  <div class="toggle-row"><div class="toggle-text"><strong>{{ t('Email Notifications') }}</strong><span>{{ t('Receive updates via email') }}</span></div><mat-slide-toggle formControlName="emailNotifications" color="primary"></mat-slide-toggle></div>
                  <div class="toggle-row"><div class="toggle-text"><strong>{{ t('Push Notifications') }}</strong><span>{{ t('Browser push notifications') }}</span></div><mat-slide-toggle formControlName="pushNotifications" color="primary"></mat-slide-toggle></div>
                  <div class="toggle-row"><div class="toggle-text"><strong>{{ t('SMS Notifications') }}</strong><span>{{ t('Get alerts via SMS') }}</span></div><mat-slide-toggle formControlName="smsNotifications" color="primary"></mat-slide-toggle></div>
                </div>

                <div class="form-actions">
                  <button mat-flat-button color="primary" type="submit" [disabled]="savingPrefs">{{ t('Save Preferences') }}</button>
                </div>
              </form>
            </div>
          </mat-tab>

          <mat-tab [label]="t('Security')">
            <div class="tab-inner">
              <h3 class="section-title"><mat-icon>lock</mat-icon> {{ t('Change Password') }}</h3>
              <form [formGroup]="passwordForm" (ngSubmit)="changePassword()">
                <div class="grid-2">
                  <mat-form-field appearance="outline"><mat-label>{{ t('Current Password') }}</mat-label><input matInput [type]="hidePass?'password':'text'" formControlName="currentPassword"><button mat-icon-button matSuffix type="button" (click)="hidePass=!hidePass"><mat-icon>{{hidePass?'visibility_off':'visibility'}}</mat-icon></button></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('New Password') }}</mat-label><input matInput [type]="hidePass?'password':'text'" formControlName="newPassword"></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Confirm New Password') }}</mat-label><input matInput [type]="hidePass?'password':'text'" formControlName="confirmNewPassword"></mat-form-field>
                </div>
                <div class="form-actions"><button mat-flat-button color="primary" type="submit" [disabled]="savingPass">{{ t('Change Password') }}</button></div>
              </form>

              <mat-divider></mat-divider>
              <h3 class="section-title"><mat-icon>security</mat-icon> {{ t('Two-Factor Authentication') }}</h3>
              <div class="toggle-row"><div class="toggle-text"><strong>{{ t('Enable 2FA') }}</strong><span>{{ t('Add extra security') }}</span></div><mat-slide-toggle [checked]="profile?.twoFactorEnabled" (change)="toggleTwoFactor($event)" color="primary"></mat-slide-toggle></div>

              <mat-divider></mat-divider>
              <h3 class="section-title"><mat-icon>help</mat-icon> {{ t('Security Question') }}</h3>
              <form [formGroup]="securityForm" (ngSubmit)="saveSecurityQuestion()">
                <div class="grid-2">
                  <mat-form-field appearance="outline"><mat-label>{{ t('Question') }}</mat-label><mat-select formControlName="question"><mat-option value="What is your pet's name?">What is your pet's name?</mat-option><mat-option value="What is your mother's maiden name?">What is your mother's maiden name?</mat-option><mat-option value="What city were you born in?">What city were you born in?</mat-option><mat-option value="What is your favorite book?">What is your favorite book?</mat-option></mat-select></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Answer') }}</mat-label><input matInput formControlName="answer"></mat-form-field>
                  <mat-form-field appearance="outline"><mat-label>{{ t('Current Password') }}</mat-label><input matInput type="password" formControlName="password"></mat-form-field>
                </div>
                <div class="form-actions"><button mat-flat-button color="primary" type="submit" [disabled]="savingSec">{{ t('Save Security Question') }}</button></div>
              </form>
            </div>
          </mat-tab>

          <mat-tab [label]="t('Sessions')">
            <div class="tab-inner">
              <div class="sessions-list" *ngIf="sessions.length > 0; else noSessions">
                <div class="session-card" *ngFor="let s of sessions" [class.current]="s.isCurrent">
                  <div class="session-left"><mat-icon>{{ s.isCurrent ? 'devices' : 'phone_android' }}</mat-icon><div><strong>{{ s.deviceInfo }} {{ s.isCurrent ? '(' + t('Current') + ')' : '' }}</strong><span>{{ s.ipAddress }} • {{ s.location }} • {{ t('Last active') }}: {{ formatDate(s.lastActivity) }}</span></div></div>
                  <button mat-icon-button color="warn" (click)="revokeSession(s)" *ngIf="!s.isCurrent" [matTooltip]="t('Log out this device')"><mat-icon>logout</mat-icon></button>
                </div>
              </div>
              <ng-template #noSessions><div class="empty"><mat-icon>devices</mat-icon><p>{{ t('No active sessions') }}</p></div></ng-template>
            </div>
          </mat-tab>

          <mat-tab [label]="t('Activity Log')">
            <div class="tab-inner">
              <div class="activity-list" *ngIf="activities.length > 0; else noActivity">
                <div class="activity-row" *ngFor="let a of activities">
                  <div class="activity-dot" [style.background]="activityColor(a.activityType)"></div>
                  <mat-icon [style.color]="activityColor(a.activityType)">{{ activityIcon(a.activityType) }}</mat-icon>
                  <div class="activity-text"><strong>{{ a.description }}</strong><span>{{ a.ipAddress }} • {{ formatDate(a.createdAt) }}</span></div>
                </div>
              </div>
              <ng-template #noActivity><div class="empty"><mat-icon>history</mat-icon><p>{{ t('No recent activity') }}</p></div></ng-template>
            </div>
          </mat-tab>

        </mat-tab-group>
      </mat-card>
    </div>
  `,
  styles: [`
    .page { max-width: 960px; margin: 0 auto; padding: 24px; }
    .top-bar-loader { position: fixed; top: 0; left: 0; right: 0; z-index: 999; }
    .header-card { background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.06); margin-bottom: 24px; }
    .cover { height: 100px; background: linear-gradient(135deg, #0d47a1, #1a73e8, #64b5f6); }
    .header-body { padding: 0 28px 24px; margin-top: -50px; display: flex; align-items: flex-end; gap: 24px; flex-wrap: wrap; }
    .avatar-zone { position: relative; width: 100px; height: 100px; min-width: 100px; border-radius: 50%; cursor: pointer; border: 4px solid white; overflow: hidden; }
    .avatar-img { width: 100%; height: 100%; object-fit: cover; display: block; }
    .avatar-placeholder { width: 100%; height: 100%; background: linear-gradient(135deg, #1a73e8, #64b5f6); display: flex; align-items: center; justify-content: center; font-size: 2rem; font-weight: 700; color: white; }
    .avatar-hover { position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.55); display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 2px; opacity: 0; transition: opacity 0.2s; color: white; font-size: 0.7rem; }
    .avatar-hover mat-icon { font-size: 24px; width: 24px; height: 24px; }
    .avatar-zone:hover .avatar-hover { opacity: 1; }
    .header-info { flex: 1; padding-top: 50px; }
    .header-info h1 { font-size: 1.5rem; font-weight: 700; margin: 0 0 2px; }
    .subtitle { color: #666; margin: 0 0 8px; font-size: 0.9rem; }
    .badges { display: flex; gap: 8px; margin-bottom: 8px; }
    .badge { padding: 3px 12px; border-radius: 12px; font-size: 0.72rem; font-weight: 600; }
    .badge.superadmin { background: #f3e5f5; color: #6a1b9a; }
    .badge.admin { background: #e3f2fd; color: #1565c0; }
    .badge.test { background: #e8f5e9; color: #2e7d32; }
    .badge.viewer { background: #f5f5f5; color: #616161; }
    .badge-status { padding: 3px 12px; border-radius: 12px; font-size: 0.72rem; font-weight: 600; background: #fce8e6; color: #c5221f; }
    .badge-status.active { background: #e6f4ea; color: #137333; }
    .meta { display: flex; gap: 16px; flex-wrap: wrap; }
    .meta span { display: flex; align-items: center; gap: 4px; font-size: 0.82rem; color: #666; }
    .meta mat-icon { font-size: 16px; width: 16px; height: 16px; color: #999; }
    .tabs-card { border-radius: 16px; box-shadow: 0 2px 12px rgba(0,0,0,0.06); }
    .tab-inner { padding: 24px; }
    .section-title { display: flex; align-items: center; gap: 8px; font-size: 1rem; font-weight: 600; color: #333; margin: 0 0 16px; }
    .section-title mat-icon { color: #1a73e8; font-size: 20px; width: 20px; height: 20px; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .form-actions { display: flex; justify-content: flex-end; margin-top: 16px; }
    mat-form-field { width: 100%; }
    mat-divider { margin: 24px 0; }
    .toggle-list { display: flex; flex-direction: column; }
    .toggle-row { display: flex; align-items: center; justify-content: space-between; padding: 14px 0; border-bottom: 1px solid #f0f0f0; }
    .toggle-text strong { display: block; font-size: 0.9rem; }
    .toggle-text span { color: #888; font-size: 0.78rem; }
    .sessions-list { display: flex; flex-direction: column; gap: 8px; }
    .session-card { display: flex; align-items: center; justify-content: space-between; padding: 14px 16px; background: #f9fafb; border-radius: 10px; }
    .session-card.current { background: #e8f5e9; border: 1px solid #c8e6c9; }
    .session-left { display: flex; align-items: center; gap: 12px; }
    .session-left mat-icon { color: #1a73e8; }
    .session-left strong { display: block; font-size: 0.9rem; }
    .session-left span { color: #888; font-size: 0.78rem; }
    .activity-list { display: flex; flex-direction: column; }
    .activity-row { display: flex; align-items: center; gap: 12px; padding: 12px 8px; border-bottom: 1px solid #f5f5f5; }
    .activity-dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
    .activity-text strong { display: block; font-size: 0.88rem; }
    .activity-text span { color: #888; font-size: 0.78rem; }
    .empty { text-align: center; padding: 40px; color: #888; }
    .empty mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; margin-bottom: 8px; }
    @media (max-width: 768px) { .page { padding: 16px; } .grid-2 { grid-template-columns: 1fr; } .header-body { flex-direction: column; align-items: center; text-align: center; margin-top: -40px; } .header-info { padding-top: 0; } .meta { justify-content: center; } .badges { justify-content: center; } }
  `]
})
export class ProfileComponent implements OnInit {
  private profileSrv = inject(ProfileService);
  private auth = inject(AuthService);
  readonly langSrv = inject(LanguageService);
  private notify = inject(NotificationService);
  private fb = inject(FormBuilder);

  profile: any = null;
  sessions: any[] = [];
  activities: any[] = [];
  loading = true;
  savingProfile = false;
  savingPrefs = false;
  savingPass = false;
  savingSec = false;
  hidePass = true;
  profilePictureUrl: string = '';
  selectedTab = 0;
  baseUrl = environment.apiUrl;

  profileForm = this.fb.group({ firstName: [''], lastName: [''], phoneNumber: [''], bio: [''], jobTitle: [''], department: [''], company: [''] });
  prefsForm = this.fb.group({ timezone: ['Asia/Kathmandu'], language: ['en'], theme: ['light'], emailNotifications: [true], pushNotifications: [true], smsNotifications: [false] });
  passwordForm = this.fb.group({ currentPassword: ['', Validators.required], newPassword: ['', [Validators.required, Validators.minLength(8)]], confirmNewPassword: ['', Validators.required] });
  securityForm = this.fb.group({ question: ['What is your pet\'s name?'], answer: [''], password: ['', Validators.required] });

  // ========== USER-SPECIFIC STORAGE KEY ==========
  private get userPictureKey(): string {
    const userId = this.auth.getCurrentUser()?.id || 'anonymous';
    return `profilePicture_${userId}`;
  }

  // Cache busting timestamp to prevent browser caching issues
  private get cacheBuster(): string {
    return `?t=${Date.now()}`;
  }

  t(key: string): string { return this.langSrv.t(key); }
  getInitials(): string { return this.profile ? `${this.profile.firstName?.charAt(0) || ''}${this.profile.lastName?.charAt(0) || ''}`.toUpperCase() : '?'; }
  getHighestRole(): string { return this.auth.getHighestRole() || 'Viewer'; }

  ngOnInit(): void {
    if (localStorage.getItem('theme') === 'dark') document.body.classList.add('dark-theme');
    this.loadProfile();
  }

  private loadProfilePicture(): void {
    // First, try to get the picture from the server profile data
    if (this.profile?.profilePictureUrl) {
      // If the URL is relative, prepend the base URL
      if (this.profile.profilePictureUrl.startsWith('http')) {
        this.profilePictureUrl = this.profile.profilePictureUrl + this.cacheBuster;
      } else {
        this.profilePictureUrl = this.baseUrl + this.profile.profilePictureUrl + this.cacheBuster;
      }
      // Update localStorage with server URL for consistency
      localStorage.setItem(this.userPictureKey, this.profilePictureUrl);
    } else {
      // Fallback to localStorage
      const savedPic = localStorage.getItem(this.userPictureKey);
      if (savedPic && savedPic !== 'undefined' && savedPic !== 'null') {
        // Check if it's a data URL (local upload) or server URL
        if (savedPic.startsWith('data:') || savedPic.startsWith('http')) {
          this.profilePictureUrl = savedPic;
        } else {
          // It's a relative path from server
          this.profilePictureUrl = this.baseUrl + savedPic;
        }
      }
    }
  }

  loadProfile(): void {
    this.loading = true;
    this.profileSrv.getProfile().subscribe({
      next: (r: any) => {
        this.loading = false;
        if (r.success && r.data) {
          this.profile = r.data;
          
          // Load profile picture first
          this.loadProfilePicture();
          
          this.profileForm.patchValue({ 
            firstName: r.data.firstName || '', 
            lastName: r.data.lastName || '', 
            phoneNumber: r.data.phoneNumber || '', 
            bio: r.data.bio || '', 
            jobTitle: r.data.jobTitle || '', 
            department: r.data.department || '', 
            company: r.data.company || '' 
          });
          this.profileForm.markAsPristine();
          
          this.prefsForm.patchValue({ 
            timezone: r.data.timezone || 'Asia/Kathmandu', 
            language: r.data.language || 'en', 
            theme: r.data.theme || 'light', 
            emailNotifications: r.data.emailNotifications ?? true, 
            pushNotifications: r.data.pushNotifications ?? true, 
            smsNotifications: r.data.smsNotifications ?? false 
          });
          this.prefsForm.markAsPristine();
          
          this.sessions = r.data.activeSessions || [];
          this.activities = r.data.recentActivities || [];
        } else {
          // If no server data, try localStorage fallback
          this.loadProfilePicture();
        }
      },
      error: () => { 
        this.loading = false; 
        // On error, try to load from localStorage
        this.loadProfilePicture();
      }
    });
  }

  onTabChange(event: any): void { 
    this.selectedTab = event.index; 
    if (event.index === 3) this.loadSessions(); 
    if (event.index === 4) this.loadActivities(); 
  }

  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (!file) return;
    
    // Validate file type
    if (!file.type.startsWith('image/')) { 
      this.notify.error('Please select an image file'); 
      return; 
    }
    
    // Validate file size (10MB limit)
    if (file.size > 10 * 1024 * 1024) { 
      this.notify.error('File too large (max 10MB)'); 
      return; 
    }

    // Show loading state
    this.loading = true;

    // Convert to data URL for immediate preview
    const reader = new FileReader();
    reader.onload = (e: any) => {
      const dataUrl = e.target.result;
      // Set preview immediately
      this.profilePictureUrl = dataUrl;
    };
    reader.readAsDataURL(file);

    // Upload to server for persistent storage
    this.profileSrv.uploadPicture(file).subscribe({
      next: (response: any) => {
        this.loading = false;
        if (response.success) {
          // If server returns the new picture URL, use it
          if (response.data?.profilePictureUrl) {
            const serverUrl = response.data.profilePictureUrl.startsWith('http') 
              ? response.data.profilePictureUrl 
              : this.baseUrl + response.data.profilePictureUrl;
            this.profilePictureUrl = serverUrl + this.cacheBuster;
            // Store the server URL in localStorage for offline fallback
            localStorage.setItem(this.userPictureKey, serverUrl);
          } else {
            // If server doesn't return URL, keep the data URL and store it
            localStorage.setItem(this.userPictureKey, this.profilePictureUrl);
          }
          
          this.notify.success('Profile picture updated successfully!');
          
          // Reload profile to get updated data
          this.loadProfile();
        } else {
          this.notify.error(response.message || 'Failed to update profile picture');
          // Reload to revert to server state
          this.loadProfile();
        }
      },
      error: (error: any) => {
        this.loading = false;
        console.error('Failed to upload profile picture:', error);
        this.notify.warning('Failed to upload to server. Picture saved locally.');
        // Keep the local preview but store in localStorage
        localStorage.setItem(this.userPictureKey, this.profilePictureUrl);
      }
    });
    
    // Clear the file input
    event.target.value = '';
  }

  saveProfile(): void { 
    if (this.profileForm.invalid || !this.profileForm.dirty) return; 
    this.savingProfile = true; 
    this.profileSrv.updateProfile(this.profileForm.value).subscribe({ 
      next: (r: any) => { 
        this.savingProfile = false; 
        if (r.success) { 
          this.notify.success(this.t('Profile updated!')); 
          this.loadProfile(); 
        } else {
          this.notify.error(r.message || 'Failed to update profile');
        }
      }, 
      error: () => { 
        this.savingProfile = false; 
        this.notify.error('Failed to update profile'); 
      } 
    }); 
  }

  savePreferences(): void {
    this.savingPrefs = true; 
    const prefs = this.prefsForm.value;
    this.profileSrv.updatePreferences(prefs).subscribe({
      next: () => { 
        this.savingPrefs = false; 
        this.applyPreferences(prefs); 
        this.notify.success(prefs.language === 'ne' ? 'प्राथमिकताहरू सुरक्षित गरियो!' : 'Preferences saved!'); 
      },
      error: () => { 
        this.savingPrefs = false; 
        this.applyPreferences(prefs); 
        this.notify.warning(prefs.language === 'ne' ? 'स्थानीय रूपमा सुरक्षित' : 'Saved locally'); 
      }
    });
  }

  private applyPreferences(prefs: any): void {
    if (prefs.theme === 'dark') { 
      document.body.classList.add('dark-theme'); 
      localStorage.setItem('theme', 'dark'); 
    } else { 
      document.body.classList.remove('dark-theme'); 
      localStorage.setItem('theme', 'light'); 
    }
    this.langSrv.setLanguage(prefs.language === 'ne' ? 'ne' : 'en');
    if (prefs.timezone) { 
      moment.tz.setDefault(prefs.timezone); 
      localStorage.setItem('timezone', prefs.timezone); 
    }
    this.prefsForm.markAsPristine();
  }

  changePassword(): void { 
    if (this.passwordForm.invalid || this.passwordForm.value.newPassword !== this.passwordForm.value.confirmNewPassword) { 
      this.notify.error(this.t('Passwords do not match')); 
      return; 
    } 
    this.savingPass = true; 
    this.profileSrv.changePassword(this.passwordForm.value).subscribe({ 
      next: (r: any) => { 
        this.savingPass = false; 
        if (r.success) { 
          this.notify.success(this.t('Password changed!')); 
          this.passwordForm.reset(); 
        } else {
          this.notify.error(r.message || 'Failed to change password'); 
        }
      }, 
      error: () => { 
        this.savingPass = false; 
        this.notify.error('Failed to change password'); 
      } 
    }); 
  }

  saveSecurityQuestion(): void { 
    this.savingSec = true; 
    this.profileSrv.updateSecurityQuestion(this.securityForm.value).subscribe({ 
      next: (r: any) => { 
        this.savingSec = false; 
        if (r.success) { 
          this.notify.success(this.t('Saved!')); 
          this.securityForm.markAsPristine(); 
        } else {
          this.notify.error(r.message || 'Failed to save security question'); 
        }
      }, 
      error: () => { 
        this.savingSec = false; 
        this.notify.error('Failed to save security question'); 
      } 
    }); 
  }

  toggleTwoFactor(event: any): void { 
    const enable = event.checked; 
    this.notify.showPrompt(
      enable ? 'Enable 2FA' : 'Disable 2FA', 
      'Enter your password', 
      'Password'
    ).then(password => { 
      if (!password) return; 
      this.profileSrv.updateTwoFactor({ enable, password }).subscribe({ 
        next: (r: any) => { 
          if (r.success) { 
            this.notify.success(enable ? '2FA enabled!' : '2FA disabled!'); 
            this.loadProfile(); 
          } else {
            this.notify.error(r.message || 'Failed to update 2FA'); 
          }
        }, 
        error: () => this.notify.error('Failed to update 2FA') 
      }); 
    }); 
  }

  loadSessions(): void { 
    this.profileSrv.getSessions().subscribe({ 
      next: (r: any) => { 
        if (r.success) this.sessions = r.data; 
      }, 
      error: () => {} 
    }); 
  }

  async revokeSession(s: any): Promise<void> { 
    const ok = await this.notify.confirmAction('Revoke Session', `Log out ${s.deviceInfo}?`); 
    if (!ok) return; 
    this.profileSrv.revokeSession(s.id).subscribe({ 
      next: (r: any) => { 
        if (r.success) { 
          this.notify.success('Session revoked!'); 
          this.loadSessions(); 
        }
      }, 
      error: () => this.notify.error('Failed to revoke session') 
    }); 
  }

  loadActivities(): void { 
    this.profileSrv.getActivities().subscribe({ 
      next: (r: any) => { 
        if (r.success) this.activities = r.data; 
      }, 
      error: () => {} 
    }); 
  }

  activityIcon(type: string): string { 
    const m: any = { 
      Login: 'login', 
      PasswordChange: 'lock_reset', 
      ProfileUpdate: 'edit', 
      Logout: 'logout', 
      SecurityChange: 'security' 
    }; 
    return m[type] || 'info'; 
  }

  activityColor(type: string): string { 
    const m: any = { 
      Login: '#137333', 
      PasswordChange: '#f57c00', 
      ProfileUpdate: '#1976d2', 
      Logout: '#757575', 
      SecurityChange: '#c5221f' 
    }; 
    return m[type] || '#757575'; 
  }

  formatDate(d: string): string { 
    return d ? moment(d).format('DD/MM/YYYY HH:mm') : '-'; 
  }
}