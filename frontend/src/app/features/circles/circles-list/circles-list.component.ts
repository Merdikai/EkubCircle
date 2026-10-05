import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CircleService } from '../../../core/services';
import { Circle } from '../../../core/models';
import { StatusBadgeComponent, EtbCurrencyPipe } from '../../../shared';

@Component({
  selector: 'app-circles-list',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgeComponent, EtbCurrencyPipe],
  templateUrl: './circles-list.component.html',
  styleUrls: ['./circles-list.component.css']
})
export class CirclesListComponent implements OnInit {
  private circleService = inject(CircleService);

  circles = signal<Circle[]>([]);
  activeFilter = signal<'All' | 'Active' | 'Forming' | 'Completed'>('All');
  isLoading = signal<boolean>(true);

  filteredCircles = computed(() => {
    const f = this.activeFilter();
    if (f === 'All') return this.circles();
    return this.circles().filter(c => c.status === f);
  });

  ngOnInit(): void {
    this.loadCircles();
  }

  loadCircles(): void {
    this.isLoading.set(true);
    this.circleService.getCircles().subscribe({
      next: (list) => {
        this.circles.set(list);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  setFilter(filter: 'All' | 'Active' | 'Forming' | 'Completed'): void {
    this.activeFilter.set(filter);
  }
}
