import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="badge" [ngClass]="badgeClass">
      <span class="badge-dot"></span>
      {{ status() }}
    </span>
  `,
  styles: [`
    .badge {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 4px 10px;
      border-radius: var(--radius-full);
      font-size: 11.5px;
      font-weight: 600;
      letter-spacing: 0.2px;
      text-transform: capitalize;
    }
    .badge-dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background-color: currentColor;
    }
    .badge-forming { background: #fef3c7; color: #b45309; border: 1px solid #fde68a; }
    .badge-active { background: #fcf1f2; color: #791e2a; border: 1px solid #f3d4d8; }
    .badge-completed { background: #eaf6ee; color: #1e7e48; border: 1px solid #c3e6cb; }
    .badge-paid { background: #eaf6ee; color: #1e7e48; border: 1px solid #c3e6cb; }
    .badge-open { background: #eff6ff; color: #1d4ed8; border: 1px solid #bfdbfe; }
    .badge-unpaid { background: #fef2f2; color: #b91c1c; border: 1px solid #fecaca; }
    .badge-pending { background: #fef3c7; color: #b45309; border: 1px solid #fde68a; }
  `]
})
export class StatusBadgeComponent {
  status = input.required<string>();

  get badgeClass(): string {
    const s = this.status().toLowerCase().replace(/\s+/g, '-');
    return `badge-${s}`;
  }
}
