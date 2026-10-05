import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService, CircleService } from '../../core/services';
import { Circle } from '../../core/models';
import { StatCardComponent, EtbCurrencyPipe } from '../../shared';

interface EthiopianProverb {
  text: string;
  amharic: string;
  author: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, StatCardComponent, EtbCurrencyPipe],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  private circleService = inject(CircleService);

  circles = signal<Circle[]>([]);
  isLoading = signal<boolean>(true);

  // Proverb Rotator
  proverbs: EthiopianProverb[] = [
    {
      text: 'Small contributions create big dreams.',
      amharic: 'ጥቃቅን ቁጠባዎች ታላላቅ ህልሞችን እውን ያደርጋሉ።',
      author: 'Ekub Wisdom'
    },
    {
      text: 'When spider webs unite, they can tie up a lion.',
      amharic: 'ድር ቢያብር አንበሳ ያስር።',
      author: 'Traditional Ethiopian Proverb'
    },
    {
      text: 'He who saves together with his brothers never falls into hardship.',
      amharic: 'ከወንድሞቹ ጋር የቆጠበ ችግር አይጥለውም።',
      author: 'Ekub Community'
    }
  ];

  currentProverbIndex = signal(0);
  currentProverb = computed(() => this.proverbs[this.currentProverbIndex()]);

  // Dynamic signals reacting to real state
  activeCircles = computed(() => this.circles().filter(c => c.status === 'Active'));
  
  totalSaved = computed(() => {
    return this.circles().reduce((sum, c) => sum + (c.totalSaved ?? 0), 0);
  });

  availableBalance = computed(() => {
    return this.authService.currentUser()?.walletBalance ?? 3200;
  });

  monthlyContributions = signal<number>(8300);

  activeGroupsCount = computed(() => this.activeCircles().length);
  totalGroupsCount = computed(() => this.circles().length);

  ngOnInit(): void {
    this.loadCircles();
  }

  loadCircles(): void {
    this.isLoading.set(true);
    this.circleService.getCircles().subscribe({
      next: (data) => {
        this.circles.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  nextProverb(): void {
    this.currentProverbIndex.update(i => (i + 1) % this.proverbs.length);
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }
}
