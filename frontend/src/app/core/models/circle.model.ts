export type CircleStatus = 'Forming' | 'Active' | 'Completed';

export interface Circle {
  id: number;
  name: string;
  contributionAmount: number; // In ETB / Birr
  meetingLabel: string; // e.g. "Weekly on Sundays", descriptive label only
  status: CircleStatus;
  createdByUserId: number;
  createdAt: string;
  startedAt?: string | null;
  completedAt?: string | null;
  
  // Computed / UI metadata fields
  memberCount?: number;
  targetAmount?: number;
  totalSaved?: number;
  currentRoundNumber?: number;
  nextContributionDays?: number;
  bannerImage?: string;
  accentColor?: 'burgundy' | 'blue' | 'charcoal';
}

export interface CreateCircleRequest {
  name: string;
  contributionAmount: number;
  meetingLabel: string;
  durationMonths?: number;
  targetAmount?: number;
}

export interface StartCircleResponse {
  circleId: number;
  status: CircleStatus;
  startedAt: string;
  totalRounds: number;
  firstRoundId: number;
  message: string;
}
