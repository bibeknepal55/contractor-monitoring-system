import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-error-state',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  template: `
    <div class="error-state">
      <mat-icon class="error-icon">error_outline</mat-icon>
      <h3 class="error-title">{{ title }}</h3>
      <p class="error-message">{{ message }}</p>
      <button mat-stroked-button color="primary" (click)="retry.emit()" *ngIf="retryable">
        <mat-icon>refresh</mat-icon> Retry
      </button>
    </div>
  `,
  styles: [`
    .error-state {
      text-align: center;
      padding: 60px 20px;
      color: #888;
    }
    .error-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #f59e0b;
      margin-bottom: 12px;
    }
    .error-title {
      margin: 0 0 6px;
      font-size: 1.1rem;
      font-weight: 600;
      color: #555;
    }
    .error-message {
      margin: 0 auto 16px;
      max-width: 400px;
      font-size: 0.88rem;
      color: #999;
      line-height: 1.5;
    }
  `]
})
export class ErrorStateComponent {
  @Input() title = 'Something went wrong';
  @Input() message = 'An error occurred while loading data.';
  @Input() retryable = true;
  @Output() retry = new EventEmitter<void>();
}