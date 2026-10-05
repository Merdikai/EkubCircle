import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { EkubNotification } from '../models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private notificationsSignal = signal<EkubNotification[]>([]);

  readonly notifications = this.notificationsSignal.asReadonly();

  getNotifications(): Observable<EkubNotification[]> {
    return this.http.get<EkubNotification[]>('/api/notifications').pipe(
      tap(items => this.notificationsSignal.set(items))
    );
  }

  markAsRead(id: number): Observable<void> {
    return this.http.post<void>(`/api/notifications/${id}/read`, {}).pipe(
      tap(() => {
        this.notificationsSignal.update(list =>
          list.map(n => n.id === id ? { ...n, isRead: true } : n)
        );
      })
    );
  }
}
