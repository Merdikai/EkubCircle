export type PaymentStatus = 'Completed' | 'Pending' | 'Failed';
export type PaymentType = 'Normal' | 'Extra';

export interface Payment {
  id: number;
  roundId: number;
  circleMemberId: number;
  amount: number;
  paymentType: PaymentType;
  status: PaymentStatus;
  paidAt: string;
  
  // UI helpers
  memberName?: string;
  memberAvatar?: string;
  circleName?: string;
  roundNumber?: number;
}

export interface RecordPaymentRequest {
  circleId: number;
  roundId: number;
  circleMemberId: number;
  amount?: number; // Defaults to circle's fixed contribution amount
  paymentType?: PaymentType;
}

export interface PaymentHistoryFilter {
  circleId?: number;
  roundId?: number;
  month?: string; // e.g., '2025-04' or 'April 2025'
  status?: PaymentStatus;
}
