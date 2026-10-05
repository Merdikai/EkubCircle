import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-mobile-preview-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mobile-preview-modal.component.html',
  styleUrls: ['./mobile-preview-modal.component.css']
})
export class MobilePreviewModalComponent {
  isOpen = input<boolean>(false);
  close = output<void>();

  activePhoneScreen = signal<'onboarding' | 'dashboard'>('onboarding');

  setScreen(s: 'onboarding' | 'dashboard'): void {
    this.activePhoneScreen.set(s);
  }

  onClose(): void {
    this.close.emit();
  }
}
