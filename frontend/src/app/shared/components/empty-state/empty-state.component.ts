import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, RouterLink],
  template: `
    <div class="empty-state">
      <mat-icon class="empty-icon">{{ icon }}</mat-icon>
      <h3 class="empty-title">{{ title }}</h3>
      <p class="empty-description">{{ description }}</p>
      <ng-content></ng-content>
      <button *ngIf="actionLabel && actionRoute" 
        mat-flat-button color="primary" 
        [routerLink]="actionRoute" 
        class="empty-action">
        <mat-icon>{{ actionIcon || 'add' }}</mat-icon>
        {{ actionLabel }}
      </button>
    </div>
  `,
  styles: [`
    .empty-state {
      text-align: center;
      padding: 60px 20px;
      color: #888;
    }
    .empty-icon {
      font-size: 56px;
      width: 56px;
      height: 56px;
      color: #d1d5db;
      margin-bottom: 16px;
    }
    .empty-title {
      margin: 0 0 8px;
      font-size: 1.15rem;
      font-weight: 600;
      color: #555;
    }
    .empty-description {
      margin: 0 auto 20px;
      max-width: 400px;
      font-size: 0.9rem;
      color: #999;
      line-height: 1.5;
    }
    .empty-action {
      border-radius: 8px;
      font-weight: 500;
    }
  `]
})
export class EmptyStateComponent {
  @Input() icon: string = 'inbox';
  @Input() title: string = 'No Data Found';
  @Input() description: string = '';
  @Input() actionLabel: string = '';
  @Input() actionRoute: string = '';
  @Input() actionIcon: string = 'add';
}