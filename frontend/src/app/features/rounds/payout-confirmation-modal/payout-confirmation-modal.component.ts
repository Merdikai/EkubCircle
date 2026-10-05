import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Round, Circle } from '../../../core/models';
import { EtbCurrencyPipe } from '../../../shared';

@Component({
  selector: 'app-payout-confirmation-modal',
  standalone: true,
  imports: [CommonModule, EtbCurrencyPipe],
  templateUrl: './payout-confirmation-modal.component.html',
  styleUrls: ['./payout-confirmation-modal.component.css']
})
export class PayoutConfirmationModalComponent {
  isOpen = input<boolean>(false);
  round = input.required<Round>();
  circle = input.required<Circle>();
  isLoading = input<boolean>(false);

  confirmPayout = output<void>();
  cancel = output<void>();

  onConfirm(): void {
    this.confirmPayout.emit();
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
