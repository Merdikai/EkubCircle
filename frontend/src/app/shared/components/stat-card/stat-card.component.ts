import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stat-card" [ngClass]="'theme-' + theme()">
      <div class="stat-body">
        <span class="stat-label">{{ label() }}</span>
        <div class="stat-value-row">
          <h3 class="stat-value">{{ value() }}</h3>
        </div>
        <p class="stat-subtext">{{ subtext() }}</p>
      </div>

      <div class="stat-icon-wrap">
        <ng-content select="[icon]"></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .stat-card {
      border-radius: var(--radius-xl);
      padding: var(--space-5) var(--space-6);
      display: flex;
      align-items: center;
      justify-content: space-between;
      position: relative;
      overflow: hidden;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      min-height: 120px;
    }

    .stat-card:hover {
      transform: translateY(-2px);
    }

    .stat-body {
      display: flex;
      flex-direction: column;
      gap: 4px;
      z-index: 2;
    }

    .stat-label {
      font-size: 13px;
      font-weight: 600;
      opacity: 0.9;
    }

    .stat-value {
      font-size: 24px;
      font-weight: 800;
      letter-spacing: -0.5px;
      line-height: 1.15;
    }

    .stat-subtext {
      font-size: 11.5px;
      opacity: 0.75;
      margin-top: 2px;
    }

    .stat-icon-wrap {
      width: 48px;
      height: 48px;
      border-radius: var(--radius-lg);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      z-index: 2;
    }

    /* Theme Burgundy (Total Saved) */
    .theme-burgundy {
      background: linear-gradient(135deg, #791e2a 0%, #63141f 100%);
      color: #ffffff;
      box-shadow: 0 8px 20px rgba(121, 30, 42, 0.22);
    }
    .theme-burgundy .stat-icon-wrap {
      background: rgba(255, 255, 255, 0.15);
      border: 1px solid rgba(255, 255, 255, 0.2);
      color: #ffffff;
    }

    /* Theme Blue (Available Balance) */
    .theme-blue {
      background: linear-gradient(135deg, #3b6b91 0%, #2f5675 100%);
      color: #ffffff;
      box-shadow: 0 8px 20px rgba(59, 107, 145, 0.22);
    }
    .theme-blue .stat-icon-wrap {
      background: rgba(255, 255, 255, 0.15);
      border: 1px solid rgba(255, 255, 255, 0.2);
      color: #ffffff;
    }

    /* Theme White (Default / Contributions / Active Groups) */
    .theme-white {
      background: #ffffff;
      color: var(--color-text-main);
      border: 1px solid var(--color-border-subtle);
      box-shadow: var(--shadow-card);
    }
    .theme-white .stat-label {
      color: var(--color-text-muted);
    }
    .theme-white .stat-subtext {
      color: var(--color-text-light);
    }
    .theme-white .stat-icon-wrap {
      background: var(--color-bg-app);
      border: 1px solid var(--color-border-subtle);
      color: var(--color-primary);
    }
  `]
})
export class StatCardComponent {
  label = input.required<string>();
  value = input.required<string>();
  subtext = input<string>('');
  theme = input<'burgundy' | 'blue' | 'white'>('white');
}
