import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, RouterLink],
  template: `
    <div class="page-header">
      <div class="page-header-left">
        <button
          *ngIf="backRoute"
          mat-icon-button
          [routerLink]="backRoute"
          class="back-button"
        >
          <mat-icon>arrow_back</mat-icon>
        </button>
        <div>
          <h1 class="page-title">{{ title }}</h1>
          <p class="page-subtitle" *ngIf="subtitle">{{ subtitle }}</p>
        </div>
      </div>
      <div class="page-header-right">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 24px;
      background: white;
      border-bottom: 1px solid #e0e0e0;
      flex-wrap: wrap;
      gap: 16px;
    }

    .page-header-left {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .back-button {
      flex-shrink: 0;
    }

    .page-title {
      font-size: 1.35rem;
      font-weight: 700;
      color: #212121;
      margin: 0;
      line-height: 1.3;
    }

    .page-subtitle {
      font-size: 0.85rem;
      color: #757575;
      margin: 2px 0 0;
    }

    .page-header-right {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    @media (max-width: 600px) {
      .page-header {
        padding: 16px;
      }

      .page-title {
        font-size: 1.1rem;
      }

      .page-header-right {
        width: 100%;
      }
    }
  `]
})
export class PageHeaderComponent {
  @Input() title: string = '';
  @Input() subtitle: string = '';
  @Input() backRoute: string = '';
}