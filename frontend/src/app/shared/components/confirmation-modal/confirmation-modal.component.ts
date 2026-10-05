import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirmation-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isOpen()) {
      <div class="modal-backdrop" (click)="onCancel()">
        <div class="modal-card" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <div class="modal-icon-wrap" [ngClass]="'icon-' + variant()">
              <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="2">
                @if (variant() === 'danger') {
                  <circle cx="12" cy="12" r="10"/>
                  <line x1="12" y1="8" x2="12" y2="12"/>
                  <line x1="12" y1="16" x2="12.01" y2="16"/>
                } @else {
                  <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
                }
              </svg>
            </div>
            <div class="modal-titles">
              <h3 class="modal-title">{{ title() }}</h3>
              <p class="modal-subtitle">{{ message() }}</p>
            </div>
          </div>

          <div class="modal-body">
            <ng-content></ng-content>
          </div>

          <div class="modal-actions">
            <button class="btn btn-outline" (click)="onCancel()" [disabled]="isLoading()">
              {{ cancelText() }}
            </button>
            <button
              class="btn"
              [ngClass]="variant() === 'danger' ? 'btn-danger' : 'btn-primary'"
              (click)="onConfirm()"
              [disabled]="isLoading()"
            >
              @if (isLoading()) {
                <span class="spinner"></span>
              }
              {{ confirmText() }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(30, 30, 36, 0.6);
      backdrop-filter: blur(4px);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: var(--space-4);
      animation: fadeIn 0.15s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    .modal-card {
      background: #ffffff;
      border-radius: var(--radius-xl);
      max-width: 480px;
      width: 100%;
      padding: var(--space-6);
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
      animation: scaleUp 0.15s ease-out;
    }

    @keyframes scaleUp {
      from { transform: scale(0.95); opacity: 0; }
      to { transform: scale(1); opacity: 1; }
    }

    .modal-header {
      display: flex;
      align-items: flex-start;
      gap: var(--space-4);
      margin-bottom: var(--space-4);
    }

    .modal-icon-wrap {
      width: 48px;
      height: 48px;
      border-radius: var(--radius-lg);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .icon-primary { background: #fcf1f2; color: #791e2a; }
    .icon-danger { background: #fef2f2; color: #b91c1c; }

    .modal-title {
      font-size: 17px;
      font-weight: 800;
      color: var(--color-text-main);
      margin-bottom: 4px;
    }

    .modal-subtitle {
      font-size: 13px;
      color: var(--color-text-muted);
      line-height: 1.4;
    }

    .modal-body {
      margin-bottom: var(--space-6);
    }

    .modal-actions {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: var(--space-3);
    }

    .btn-danger {
      background-color: var(--color-status-danger);
      color: #ffffff;
    }
    .btn-danger:hover {
      background-color: #991b1b;
    }

    .spinner {
      width: 14px;
      height: 14px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: #ffffff;
      border-radius: 50%;
      animation: spin 0.6s linear infinite;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class ConfirmationModalComponent {
  isOpen = input<boolean>(false);
  title = input<string>('Confirm Action');
  message = input<string>('Are you sure you want to proceed?');
  confirmText = input<string>('Confirm');
  cancelText = input<string>('Cancel');
  variant = input<'primary' | 'danger'>('primary');
  isLoading = input<boolean>(false);

  confirm = output<void>();
  cancel = output<void>();

  onConfirm(): void {
    this.confirm.emit();
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
