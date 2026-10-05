import { Injectable } from '@angular/core';
import { Circle, CircleMember, Round, Payment, JoinRequest, EkubNotification } from '../models';
import { DEMO_USERS } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MockDbService {
  // In-memory relational tables
  circles: Circle[] = [
    {
      id: 1,
      name: 'Addis Family Ekub',
      contributionAmount: 1000,
      meetingLabel: 'Every Sunday at 4:00 PM',
      status: 'Active',
      createdByUserId: 1,
      createdAt: '2025-01-12T10:00:00Z',
      startedAt: '2025-01-15T12:00:00Z',
      memberCount: 12,
      targetAmount: 12000,
      totalSaved: 8000,
      currentRoundNumber: 2,
      nextContributionDays: 5,
      bannerImage: 'assets/images/addis-landmark.svg',
      accentColor: 'burgundy'
    },
    {
      id: 2,
      name: 'Habesha Friends',
      contributionAmount: 1000,
      meetingLabel: 'Bi-weekly Saturdays',
      status: 'Active',
      createdByUserId: 2,
      createdAt: '2025-02-01T09:00:00Z',
      startedAt: '2025-02-05T10:00:00Z',
      memberCount: 8,
      targetAmount: 8000,
      totalSaved: 4000,
      currentRoundNumber: 3,
      nextContributionDays: 2,
      bannerImage: 'assets/images/addis-landmark.svg',
      accentColor: 'blue'
    },
    {
      id: 3,
      name: 'Workplace 1',
      contributionAmount: 1000,
      meetingLabel: 'End of Month Salary Day',
      status: 'Active',
      createdByUserId: 1,
      createdAt: '2025-02-20T11:00:00Z',
      startedAt: '2025-03-01T08:00:00Z',
      memberCount: 6,
      targetAmount: 6000,
      totalSaved: 2500,
      currentRoundNumber: 1,
      nextContributionDays: 6,
      bannerImage: 'assets/images/addis-landmark.svg',
      accentColor: 'burgundy'
    },
    {
      id: 4,
      name: 'Arada Tech Savings',
      contributionAmount: 2500,
      meetingLabel: 'Weekly Friday Tech Catchup',
      status: 'Forming',
      createdByUserId: 1,
      createdAt: '2025-03-15T14:00:00Z',
      memberCount: 4,
      targetAmount: 25000,
      totalSaved: 0,
      currentRoundNumber: 0,
      nextContributionDays: 10,
      bannerImage: 'assets/images/addis-landmark.svg',
      accentColor: 'burgundy'
    },
    {
      id: 5,
      name: 'Bole Diaspora Circle',
      contributionAmount: 5000,
      meetingLabel: 'Monthly First Monday',
      status: 'Completed',
      createdByUserId: 1,
      createdAt: '2024-06-01T10:00:00Z',
      startedAt: '2024-06-10T10:00:00Z',
      completedAt: '2024-11-10T18:00:00Z',
      memberCount: 5,
      targetAmount: 25000,
      totalSaved: 25000,
      currentRoundNumber: 5,
      nextContributionDays: 0,
      bannerImage: 'assets/images/addis-landmark.svg',
      accentColor: 'blue'
    }
  ];

  members: CircleMember[] = [
    // Addis Family Ekub (12 members)
    { id: 101, circleId: 1, userId: 1, memberOrder: 1, roleInCircle: 'Organizer', hasReceived: true, joinedAt: '2025-01-12T10:00:00Z', fullName: 'Amanuel Tesfaye', email: 'amanuel@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 102, circleId: 1, userId: 2, memberOrder: 2, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-12T11:00:00Z', fullName: 'Liya Kebede', email: 'liya@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 103, circleId: 1, userId: 3, memberOrder: 3, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-12T12:00:00Z', fullName: 'Mekonnen Abebe', email: 'mekonnen@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 104, circleId: 1, userId: 4, memberOrder: 4, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-12T13:00:00Z', fullName: 'Selamawit Tadesse', email: 'selam@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 105, circleId: 1, userId: 5, memberOrder: 5, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-12T14:00:00Z', fullName: 'Dawit Haile', email: 'dawit@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 106, circleId: 1, userId: 6, memberOrder: 6, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-12T15:00:00Z', fullName: 'Bethlehem Assefa', email: 'bethlehem@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 107, circleId: 1, userId: 7, memberOrder: 7, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-13T09:00:00Z', fullName: 'Tewodros Kassahun', email: 'teddy@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 108, circleId: 1, userId: 8, memberOrder: 8, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-13T10:00:00Z', fullName: 'Senait Desta', email: 'senait@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80', paidThisRound: true },
    { id: 109, circleId: 1, userId: 9, memberOrder: 9, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-13T11:00:00Z', fullName: 'Ermias Girma', email: 'ermias@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80', paidThisRound: false },
    { id: 110, circleId: 1, userId: 10, memberOrder: 10, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-13T12:00:00Z', fullName: 'Meron Worku', email: 'meron@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&auto=format&fit=crop&q=80', paidThisRound: false },
    { id: 111, circleId: 1, userId: 11, memberOrder: 11, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-14T08:00:00Z', fullName: 'Fitsum Gebre', email: 'fitsum@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&auto=format&fit=crop&q=80', paidThisRound: false },
    { id: 112, circleId: 1, userId: 12, memberOrder: 12, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-01-14T10:00:00Z', fullName: 'Helen Negash', email: 'helen@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&auto=format&fit=crop&q=80', paidThisRound: false },

    // Forming Circle Members (Arada Tech)
    { id: 401, circleId: 4, userId: 1, memberOrder: 1, roleInCircle: 'Organizer', hasReceived: false, joinedAt: '2025-03-15T14:00:00Z', fullName: 'Amanuel Tesfaye', email: 'amanuel@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80' },
    { id: 402, circleId: 4, userId: 2, memberOrder: 2, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-03-15T14:30:00Z', fullName: 'Liya Kebede', email: 'liya@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&auto=format&fit=crop&q=80' },
    { id: 403, circleId: 4, userId: 3, memberOrder: 3, roleInCircle: 'Member', hasReceived: false, joinedAt: '2025-03-16T10:00:00Z', fullName: 'Mekonnen Abebe', email: 'mekonnen@ekub.et', avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80' }
  ];

  rounds: Round[] = [
    // Addis Family Ekub Rounds
    {
      id: 1001,
      circleId: 1,
      roundNumber: 1,
      receiverMemberId: 101, // Amanuel Tesfaye received Round 1
      status: 'Paid Out',
      potAmount: 12000,
      paidOutAt: '2025-02-15T18:00:00Z',
      receiverName: 'Amanuel Tesfaye',
      receiverAvatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
      paidMembersCount: 12,
      totalMembersCount: 12,
      isEligibleForPayout: false
    },
    {
      id: 1002,
      circleId: 1,
      roundNumber: 2,
      receiverMemberId: 102, // Server-defined fixed receiver: Liya Kebede
      status: 'Open',
      potAmount: 8000,
      receiverName: 'Liya Kebede',
      receiverAvatar: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&auto=format&fit=crop&q=80',
      paidMembersCount: 8,
      totalMembersCount: 12,
      isEligibleForPayout: false // Cannot payout until all 12 have paid
    }
  ];

  payments: Payment[] = [
    {
      id: 5001,
      roundId: 1002,
      circleMemberId: 101,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-28T10:24:00Z',
      memberName: 'Amanuel Tesfaye',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    },
    {
      id: 5002,
      roundId: 1002,
      circleMemberId: 102,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-25T15:12:00Z',
      memberName: 'Liya Kebede',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    },
    {
      id: 5003,
      roundId: 1002,
      circleMemberId: 103,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-22T09:45:00Z',
      memberName: 'Mekonnen Abebe',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    },
    {
      id: 5004,
      roundId: 1002,
      circleMemberId: 104,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-18T11:20:00Z',
      memberName: 'Selamawit Tadesse',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    },
    {
      id: 5005,
      roundId: 1002,
      circleMemberId: 105,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-15T14:10:00Z',
      memberName: 'Dawit Haile',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    },
    {
      id: 5006,
      roundId: 1002,
      circleMemberId: 106,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-14T16:30:00Z',
      memberName: 'Bethlehem Assefa',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    },
    {
      id: 5007,
      roundId: 1002,
      circleMemberId: 107,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-12T08:20:00Z',
      memberName: 'Tewodros Kassahun',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    },
    {
      id: 5008,
      roundId: 1002,
      circleMemberId: 108,
      amount: 1000,
      paymentType: 'Normal',
      status: 'Completed',
      paidAt: '2025-04-10T12:00:00Z',
      memberName: 'Senait Desta',
      circleName: 'Addis Family Ekub',
      roundNumber: 2
    }
  ];

  joinRequests: JoinRequest[] = [
    {
      id: 701,
      circleId: 4,
      userId: 4,
      status: 'Pending',
      requestedAt: '2025-03-16T12:00:00Z',
      circleName: 'Arada Tech Savings',
      user: DEMO_USERS[3]
    }
  ];

  notifications: EkubNotification[] = [
    {
      id: 801,
      userId: 1,
      circleId: 1,
      title: 'Contribution Received',
      message: 'Amanuel Tesfaye contributed ETB 1,000 for Round 2 of Addis Family Ekub.',
      type: 'PaymentReceived',
      isRead: false,
      createdAt: '2025-04-28T10:24:00Z'
    },
    {
      id: 802,
      userId: 1,
      circleId: 1,
      title: 'Round 1 Payout Successful',
      message: 'Pot of ETB 12,000 was successfully disbursed to Amanuel Tesfaye.',
      type: 'PayoutCompleted',
      isRead: true,
      createdAt: '2025-02-15T18:05:00Z'
    }
  ];
}
