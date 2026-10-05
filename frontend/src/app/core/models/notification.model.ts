export type NotificationType = 'JoinRequest' | 'PaymentReceived' | 'RoundStarted' | 'PayoutCompleted' | 'CircleCompleted';

export interface EkubNotification {
  id: number;
  userId: number;
  circleId?: number;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
}

export interface MarkNotificationReadRequest {
  id: number;
}
