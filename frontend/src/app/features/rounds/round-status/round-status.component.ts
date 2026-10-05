import { Component, inject, signal, input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CircleService, MemberService, RoundService, PaymentService, AuthService } from '../../../core/services';
import { Circle, CircleMember, Round, Payment } from '../../../core/models';
import { StatusBadgeComponent, EtbCurrencyPipe, ToastService } from '../../../shared';
import { PayoutConfirmationModalComponent } from '../payout-confirmation-modal/payout-confirmation-modal.component';

@Component({
  selector: 'app-round-status',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, StatusBadgeComponent, EtbCurrencyPipe, PayoutConfirmationModalComponent],
  templateUrl: './round-status.component.html',
  styleUrls: ['./round-status.component.css']
})
export class RoundStatusComponent implements OnInit {
  circleId = input.required<string>();

  circleService = inject(CircleService);
  memberService = inject(MemberService);
  roundService = inject(RoundService);
  paymentService = inject(PaymentService);
  authService = inject(AuthService);
  toastService = inject(ToastService);

  circle = signal<Circle | null>(null);
  members = signal<CircleMember[]>([]);
  currentRound = signal<Round | null>(null);
  isLoading = signal<boolean>(true);

  // Record Payment Modal State
  isPaymentModalOpen = signal<boolean>(false);
  selectedMemberId = signal<number | null>(null);
  isSubmittingPayment = signal<boolean>(false);

  // Payout Modal State
  isPayoutModalOpen = signal<boolean>(false);
  isSubmittingPayout = signal<boolean>(false);

  ngOnInit(): void {
    this.loadRoundData();
  }

  loadRoundData(): void {
    const id = parseInt(this.circleId(), 10);
    this.isLoading.set(true);

    this.circleService.getCircleById(id).subscribe({
      next: (c) => {
        this.circle.set(c);
        this.memberService.getCircleMembers(id).subscribe({
          next: (m) => this.members.set(m)
        });
        this.roundService.getCurrentRound(id).subscribe({
          next: (r) => {
            this.currentRound.set(r);
            this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false)
        });
      },
      error: () => this.isLoading.set(false)
    });
  }

  // Open Payment Modal for a specific member
  openPaymentModal(memberId?: number): void {
    if (memberId) {
      this.selectedMemberId.set(memberId);
    } else {
      // Find first unpaid member
      const unpaid = this.members().find(m => !m.paidThisRound);
      this.selectedMemberId.set(unpaid ? unpaid.id : (this.members()[0]?.id ?? null));
    }
    this.isPaymentModalOpen.set(true);
  }

  closePaymentModal(): void {
    this.isPaymentModalOpen.set(false);
  }

  // Record Normal Contribution
  submitPayment(): void {
    const r = this.currentRound();
    const c = this.circle();
    const memId = this.selectedMemberId();
    if (!r || !c || !memId) return;

    this.isSubmittingPayment.set(true);

    this.paymentService.recordPayment({
      circleId: c.id,
      roundId: r.id,
      circleMemberId: memId,
      amount: c.contributionAmount,
      paymentType: 'Normal'
    }).subscribe({
      next: (payment) => {
        this.isSubmittingPayment.set(false);
        this.isPaymentModalOpen.set(false);
        this.toastService.success('Payment Recorded', `Contribution of ETB ${payment.amount} recorded successfully.`);
        
        // If current logged-in user contributed, deduct from wallet
        if (memId === 101 && this.authService.currentUser()?.id === 1) {
          this.authService.updateWalletBalance(-payment.amount);
        }

        this.loadRoundData();
      },
      error: (err) => {
        this.isSubmittingPayment.set(false);
        // NON-NEGOTIABLE RULE: A duplicate normal payment must be rejected by API and surfaced clearly in UI
        const errorDetail = err.error?.detail || 'Payment failed or was duplicate.';
        this.toastService.error('Payment Error', errorDetail);
      }
    });
  }

  // Open Payout Modal
  openPayoutModal(): void {
    this.isPayoutModalOpen.set(true);
  }

  closePayoutModal(): void {
    this.isPayoutModalOpen.set(false);
  }

  // Execute Payout (Phase 17)
  confirmPayout(): void {
    const r = this.currentRound();
    if (!r) return;

    this.isSubmittingPayout.set(true);

    this.roundService.payoutRound(r.id).subscribe({
      next: (res) => {
        this.isSubmittingPayout.set(false);
        this.isPayoutModalOpen.set(false);
        this.toastService.success('Payout Disbursed!', res.message);
        this.loadRoundData();
      },
      error: (err) => {
        this.isSubmittingPayout.set(false);
        this.toastService.error('Payout Blocked', err.error?.detail || 'Cannot disburse pot until all members contribute.');
      }
    });
  }
}
