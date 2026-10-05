import { User } from './user.model';

export type RoleInCircle = 'Organizer' | 'Member';

export interface CircleMember {
  id: number;
  circleId: number;
  userId: number;
  memberOrder: number; // Server-defined fixed payout order (1, 2, 3...)
  roleInCircle: RoleInCircle;
  hasReceived: boolean;
  joinedAt: string;
  
  // Joined navigation / UI fields
  user?: User;
  fullName?: string;
  email?: string;
  avatarUrl?: string;
  paidThisRound?: boolean;
}

export interface AddMemberRequest {
  circleId: number;
  emailOrPhone: string;
  fullName?: string;
}
