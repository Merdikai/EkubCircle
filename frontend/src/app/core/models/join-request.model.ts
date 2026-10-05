import { User } from './user.model';

export type JoinRequestStatus = 'Pending' | 'Accepted' | 'Rejected';

export interface JoinRequest {
  id: number;
  circleId: number;
  userId: number;
  status: JoinRequestStatus;
  requestedAt: string;
  reviewedAt?: string | null;
  
  user?: User;
  circleName?: string;
}

export interface ReviewJoinRequestDto {
  requestId: number;
  action: 'Accept' | 'Reject';
}

export interface CreateJoinRequestDto {
  circleId: number;
  message?: string;
}
