import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, MatProgressBarModule],
  template: `
    <div class="loading-wrapper" [class.inline]="inline">
      <mat-progress-bar *ngIf="!inline" mode="indeterminate" color="primary"></mat-progress-bar>
      <div class="loading-content" *ngIf="inline">
        <mat-progress-bar mode="indeterminate" color="primary" class="inline-bar"></mat-progress-bar>
        <span class="loading-text" *ngIf="message">{{ message }}</span>
      </div>
    </div>
  `,
  styles: [`
    .loading-wrapper { width: 100%; }
    .loading-wrapper.inline { display: flex; align-items: center; gap: 12px; padding: 8px 0; }
    .inline-bar { flex: 1; height: 4px; border-radius: 2px; }
    .loading-text { font-size: 0.82rem; color: #888; white-space: nowrap; }
  `]
})
export class LoadingSpinnerComponent {
  @Input() inline = false;
  @Input() message = '';
}