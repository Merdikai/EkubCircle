import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from './toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" aria-live="polite">
      @for (t of toastService.toasts(); track t.id) {
        <div class="toast-item" [ngClass]="'toast-' + t.type">
          <div class="toast-icon">
            @switch (t.type) {
              @case ('success') {
                <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5">
                  <polyline points="20 6 9 17 4 12"/>
                </svg>
              }
              @case ('error') {
                <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5">
                  <circle cx="12" cy="12" r="10"/>
                  <line x1="12" y1="8" x2="12" y2="12"/>
                  <line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
              }
              @case ('warning') {
                <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5">
                  <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
                  <line x1="12" y1="9" x2="12" y2="13"/>
                  <line x1="12" y1="17" x2="12.01" y2="17"/>
                </svg>
              }
              @default {
                <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5">
                  <circle cx="12" cy="12" r="10"/>
                  <line x1="12" y1="16" x2="12" y2="12"/>
                  <line x1="12" y1="8" x2="12.01" y2="8"/>
                </svg>
              }
            }
          </div>
          <div class="toast-content">
            <h5 class="toast-title">{{ t.title }}</h5>
            <p class="toast-desc">{{ t.message }}</p>
          </div>
          <button class="toast-close" (click)="toastService.dismiss(t.id)" aria-label="Close notification">
            &times;
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 24px;
      right: 24px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 380px;
      pointer-events: none;
    }

    .toast-item {
      pointer-events: auto;
      background: #ffffff;
      border-radius: var(--radius-md);
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15), 0 2px 6px rgba(0, 0, 0, 0.05);
      padding: 12px 16px;
      display: flex;
      align-items: flex-start;
      gap: 12px;
      animation: toastSlideIn 0.25s cubic-bezier(0.16, 1, 0.3, 1);
      border-left: 5px solid transparent;
    }

    @keyframes toastSlideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }

    .toast-success { border-left-color: #1e7e48; }
    .toast-success .toast-icon { color: #1e7e48; }

    .toast-error { border-left-color: #b91c1c; }
    .toast-error .toast-icon { color: #b91c1c; }

    .toast-warning { border-left-color: #d97706; }
    .toast-warning .toast-icon { color: #d97706; }

    .toast-info { border-left-color: #3b6b91; }
    .toast-info .toast-icon { color: #3b6b91; }

    .toast-icon {
      margin-top: 2px;
      flex-shrink: 0;
    }

    .toast-content {
      flex: 1;
    }

    .toast-title {
      font-size: 13.5px;
      font-weight: 700;
      color: var(--color-text-main);
      margin-bottom: 2px;
    }

    .toast-desc {
      font-size: 12px;
      color: var(--color-text-muted);
      line-height: 1.4;
    }

    .toast-close {
      background: none;
      border: none;
      font-size: 18px;
      color: var(--color-text-light);
      cursor: pointer;
      padding: 0 4px;
      line-height: 1;
    }
    .toast-close:hover {
      color: var(--color-text-main);
    }
  `]
})
export class ToastContainerComponent {
  toastService = inject(ToastService);
}
