import { Component, inject, signal, input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CircleService, RoundService, PaymentService } from '../../../core/services';
import { Circle, Round, Payment } from '../../../core/models';
import { StatusBadgeComponent, EtbCurrencyPipe } from '../../../shared';

@Component({
  selector: 'app-round-history',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, StatusBadgeComponent, EtbCurrencyPipe],
  templateUrl: './round-history.component.html',
  styleUrls: ['./round-history.component.css']
})
export class RoundHistoryComponent implements OnInit {
  circleId = input.required<string>();

  private circleService = inject(CircleService);
  private roundService = inject(RoundService);
  private paymentService = inject(PaymentService);

  circle = signal<Circle | null>(null);
  rounds = signal<Round[]>([]);
  payments = signal<Payment[]>([]);
  isLoading = signal<boolean>(true);

  selectedMonth = signal<string>('April 2025');
  selectedTab = signal<'contributions' | 'payouts'>('contributions');

  monthsList = ['April 2025', 'March 2025', 'February 2025', 'January 2025'];

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    const id = parseInt(this.circleId(), 10);
    this.isLoading.set(true);

    this.circleService.getCircleById(id).subscribe({
      next: (c) => this.circle.set(c)
    });

    this.roundService.getRounds(id).subscribe({
      next: (rList) => this.rounds.set(rList)
    });

    this.paymentService.getPayments({ circleId: id }).subscribe({
      next: (pList) => {
        this.payments.set(pList);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  setTab(tab: 'contributions' | 'payouts'): void {
    this.selectedTab.set(tab);
  }
}
