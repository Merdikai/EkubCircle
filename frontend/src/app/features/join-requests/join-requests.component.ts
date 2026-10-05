import { Component, inject, signal, OnInit, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CircleService, MemberService, AuthService } from '../../core/services';
import { Circle, JoinRequest } from '../../core/models';
import { StatusBadgeComponent, ToastService } from '../../shared';

@Component({
  selector: 'app-join-requests',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgeComponent],
  templateUrl: './join-requests.component.html',
  styleUrls: ['./join-requests.component.css']
})
export class JoinRequestsComponent implements OnInit {
  circleId = input<string>();

  private circleService = inject(CircleService);
  private memberService = inject(MemberService);
  authService = inject(AuthService);
  private toastService = inject(ToastService);

  circle = signal<Circle | null>(null);
  requests = signal<JoinRequest[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    const id = this.circleId() ? parseInt(this.circleId()!, 10) : undefined;

    if (id) {
      this.circleService.getCircleById(id).subscribe({
        next: (c) => this.circle.set(c)
      });
    }

    this.memberService.getJoinRequests(id).subscribe({
      next: (reqs) => {
        this.requests.set(reqs);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  accept(req: JoinRequest): void {
    const c = this.circle();
    if (c && c.status !== 'Forming') {
      this.toastService.error('Action Blocked', 'Circle is already active. Members cannot be added after start.');
      return;
    }

    this.memberService.reviewJoinRequest({ requestId: req.id, action: 'Accept' }).subscribe({
      next: () => {
        this.requests.update(list => list.filter(r => r.id !== req.id));
        this.toastService.success('Accepted', `${req.user?.fullName || 'User'} has joined the circle!`);
      }
    });
  }

  reject(req: JoinRequest): void {
    this.memberService.reviewJoinRequest({ requestId: req.id, action: 'Reject' }).subscribe({
      next: () => {
        this.requests.update(list => list.filter(r => r.id !== req.id));
        this.toastService.info('Declined', 'Join request was declined.');
      }
    });
  }
}
