import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatCardModule, MatTooltipModule],
  template: `
    <mat-card class="stat-card" [ngClass]="colorClass">
      <div class="stat-card-content">
        <div class="stat-card-info">
          <span class="stat-card-label">{{ label }}</span>
          <span class="stat-card-value">{{ value | number }}</span>
          <span class="stat-card-change" *ngIf="change !== undefined && change !== null">
            <mat-icon class="change-icon">{{ change >= 0 ? 'trending_up' : 'trending_down' }}</mat-icon>
            <span [class.positive]="change >= 0" [class.negative]="change < 0">
              {{ change >= 0 ? '+' : '' }}{{ change }}%
            </span>
          </span>
        </div>
        <div class="stat-card-icon">
          <mat-icon>{{ icon }}</mat-icon>
        </div>
      </div>
      <div class="stat-card-footer" *ngIf="footerText">
        <span>{{ footerText }}</span>
      </div>
    </mat-card>
  `,
  styles: [`
    .stat-card {
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
      transition: transform 0.2s, box-shadow 0.2s;
      overflow: hidden;
      border-left: 4px solid transparent;
    }

    .stat-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }

    .stat-card.primary { border-left-color: #1a73e8; }
    .stat-card.success { border-left-color: #388e3c; }
    .stat-card.warning { border-left-color: #f57c00; }
    .stat-card.danger { border-left-color: #d32f2f; }
    .stat-card.info { border-left-color: #1976d2; }
    .stat-card.purple { border-left-color: #9c27b0; }

    .stat-card-content {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      padding: 20px 20px 12px;
    }

    .stat-card-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .stat-card-label {
      font-size: 0.8rem;
      font-weight: 500;
      color: #757575;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .stat-card-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: #212121;
      line-height: 1.2;
    }

    .stat-card-change {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.8rem;
      font-weight: 500;
    }

    .change-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    .positive { color: #388e3c; }
    .negative { color: #d32f2f; }

    .stat-card-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .stat-card.primary .stat-card-icon { background: #e3f2fd; color: #1a73e8; }
    .stat-card.success .stat-card-icon { background: #e8f5e9; color: #388e3c; }
    .stat-card.warning .stat-card-icon { background: #fff3e0; color: #f57c00; }
    .stat-card.danger .stat-card-icon { background: #ffebee; color: #d32f2f; }
    .stat-card.info .stat-card-icon { background: #e3f2fd; color: #1976d2; }
    .stat-card.purple .stat-card-icon { background: #f3e5f5; color: #9c27b0; }

    .stat-card-icon mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .stat-card-footer {
      padding: 8px 20px;
      background: #fafafa;
      border-top: 1px solid #f0f0f0;
      font-size: 0.75rem;
      color: #9e9e9e;
    }

    @media (max-width: 600px) {
      .stat-card-value {
        font-size: 1.4rem;
      }

      .stat-card-content {
        padding: 16px 16px 10px;
      }
    }
  `]
})
export class StatCardComponent {
  @Input() label: string = '';
  @Input() value: number = 0;
  @Input() change: number | null = null;
  @Input() icon: string = 'info';
  @Input() colorClass: string = 'primary';
  @Input() footerText: string = '';
}