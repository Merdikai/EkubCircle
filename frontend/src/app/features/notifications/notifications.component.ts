import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NotificationService } from '../../core/services';
import { EkubNotification } from '../../core/models';
import { ToastService } from '../../shared';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css']
})
export class NotificationsComponent implements OnInit {
  private notificationService = inject(NotificationService);
  private toastService = inject(ToastService);

  notifications = signal<EkubNotification[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.isLoading.set(true);
    this.notificationService.getNotifications().subscribe({
      next: (list) => {
        this.notifications.set(list);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  markAsRead(item: EkubNotification): void {
    if (item.isRead) return;
    this.notificationService.markAsRead(item.id).subscribe({
      next: () => {
        this.notifications.update(list =>
          list.map(n => n.id === item.id ? { ...n, isRead: true } : n)
        );
        this.toastService.info('Marked as Read', item.title);
      }
    });
  }

  markAllAsRead(): void {
    this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
    this.toastService.success('Done', 'All notifications marked as read.');
  }
}
