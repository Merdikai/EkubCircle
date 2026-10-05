import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services';
import { EtbCurrencyPipe, ToastService } from '../../../shared';

interface WalletTx {
  id: number;
  title: string;
  category: string;
  date: string;
  amount: number;
  type: 'credit' | 'debit';
}

@Component({
  selector: 'app-wallet-transactions',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, EtbCurrencyPipe],
  templateUrl: './wallet-transactions.component.html',
  styleUrls: ['./wallet-transactions.component.css']
})
export class WalletTransactionsComponent {
  authService = inject(AuthService);
  private toastService = inject(ToastService);

  isDepositModalOpen = signal<boolean>(false);
  isWithdrawModalOpen = signal<boolean>(false);
  amountInput = signal<number>(1000);

  transactions: WalletTx[] = [
    {
      id: 1,
      title: 'Contribution Received',
      category: 'Addis Family Ekub',
      date: 'Apr 28, 2025 • 10:24 AM',
      amount: 1000,
      type: 'credit'
    },
    {
      id: 2,
      title: 'Payout Disbursed',
      category: 'Workplace 1',
      date: 'Apr 20, 2025 • 04:17 PM',
      amount: 2000,
      type: 'debit'
    },
    {
      id: 3,
      title: 'Contribution Received',
      category: 'Habesha Friends',
      date: 'Apr 18, 2025 • 11:03 AM',
      amount: 1000,
      type: 'credit'
    },
    {
      id: 4,
      title: 'Round 1 Payout Pot Received',
      category: 'Addis Family Ekub',
      date: 'Feb 15, 2025 • 06:00 PM',
      amount: 12000,
      type: 'credit'
    }
  ];

  openDeposit(): void {
    this.amountInput.set(1000);
    this.isDepositModalOpen.set(true);
  }

  openWithdraw(): void {
    this.amountInput.set(500);
    this.isWithdrawModalOpen.set(true);
  }

  closeModals(): void {
    this.isDepositModalOpen.set(false);
    this.isWithdrawModalOpen.set(false);
  }

  confirmDeposit(): void {
    const amt = Number(this.amountInput());
    if (amt <= 0) return;

    this.authService.updateWalletBalance(amt);
    this.transactions.unshift({
      id: Date.now(),
      title: 'Wallet Deposit',
      category: 'Direct Deposit',
      date: 'Just now',
      amount: amt,
      type: 'credit'
    });

    this.closeModals();
    this.toastService.success('Deposit Successful', `Deposited ETB ${amt} to wallet.`);
  }

  confirmWithdraw(): void {
    const amt = Number(this.amountInput());
    const balance = this.authService.currentUser()?.walletBalance ?? 0;
    if (amt <= 0) return;

    if (amt > balance) {
      this.toastService.error('Insufficient Funds', 'Cannot withdraw more than available balance.');
      return;
    }

    this.authService.updateWalletBalance(-amt);
    this.transactions.unshift({
      id: Date.now(),
      title: 'Wallet Withdrawal',
      category: 'Bank Transfer / Cashout',
      date: 'Just now',
      amount: amt,
      type: 'debit'
    });

    this.closeModals();
    this.toastService.success('Withdrawal Successful', `Withdrew ETB ${amt} from wallet.`);
  }
}
