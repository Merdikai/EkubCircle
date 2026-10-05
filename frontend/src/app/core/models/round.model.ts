export type RoundStatus = 'Open' | 'Paid Out';

export interface Round {
  id: number;
  circleId: number;
  roundNumber: number;
  receiverMemberId: number; // Server-defined fixed receiver
  status: RoundStatus;
  potAmount: number; // In ETB
  paidOutAt?: string | null;
  
  // UI helper fields
  receiverName?: string;
  receiverAvatar?: string;
  paidMembersCount?: number;
  totalMembersCount?: number;
  isEligibleForPayout?: boolean;
}

export interface PayoutRoundRequest {
  roundId: number;
  notes?: string;
}

export interface PayoutRoundResponse {
  roundId: number;
  circleId: number;
  roundNumber: number;
  receiverMemberId: number;
  receiverName: string;
  potAmount: number;
  status: RoundStatus;
  paidOutAt: string;
  nextRound?: Round | null;
  isCircleCompleted: boolean;
  message: string;
}
