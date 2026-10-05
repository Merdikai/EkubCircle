import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { of, throwError, delay } from 'rxjs';
import { MockDbService } from '../services/mock-db.service';
import { ProblemDetails } from '../models';

export const mockApiInterceptor: HttpInterceptorFn = (req, next) => {
  const url = req.url;
  const method = req.method;
  const db = inject(MockDbService);

  // Only intercept /api/* endpoints
  if (!url.startsWith('/api')) {
    return next(req);
  }

  const simulatedDelay = 200; // Realistic network latency

  // Helper for RFC 7807 ProblemDetails error
  const makeError = (status: number, title: string, detail: string, errorCode: string) => {
    const errorBody: ProblemDetails = {
      title,
      status,
      detail,
      errorCode
    };
    return throwError(() => new HttpErrorResponse({
      status,
      statusText: title,
      error: errorBody,
      url
    })).pipe(delay(simulatedDelay));
  };

  // 1. GET /api/circles
  if (url === '/api/circles' && method === 'GET') {
    return of(new HttpResponse({ status: 200, body: db.circles })).pipe(delay(simulatedDelay));
  }

  // 2. GET /api/circles/:id
  const circleMatch = url.match(/^\/api\/circles\/(\d+)$/);
  if (circleMatch && method === 'GET') {
    const id = parseInt(circleMatch[1], 10);
    const circle = db.circles.find(c => c.id === id);
    if (!circle) {
      return makeError(404, 'Not Found', `Circle with ID ${id} not found.`, 'CIRCLE_NOT_FOUND');
    }
    return of(new HttpResponse({ status: 200, body: circle })).pipe(delay(simulatedDelay));
  }

  // 3. POST /api/circles
  if (url === '/api/circles' && method === 'POST') {
    const body = req.body as any;
    const newId = db.circles.length + 1;
    const newCircle = {
      id: newId,
      name: body.name || 'New Ekub',
      contributionAmount: Number(body.contributionAmount) || 1000,
      meetingLabel: body.meetingLabel || 'Weekly on Sundays',
      status: 'Forming' as const,
      createdByUserId: 1,
      createdAt: new Date().toISOString(),
      memberCount: 1,
      targetAmount: Number(body.targetAmount) || (Number(body.contributionAmount) * 12),
      totalSaved: 0,
      currentRoundNumber: 0,
      nextContributionDays: 7,
      bannerImage: 'assets/images/addis-landmark.svg',
      accentColor: 'burgundy' as const
    };
    db.circles.unshift(newCircle);

    // NON-NEGOTIABLE RULE: The organizer is also a member. Member #1
    db.members.push({
      id: db.members.length + 100,
      circleId: newId,
      userId: 1,
      memberOrder: 1,
      roleInCircle: 'Organizer',
      hasReceived: false,
      joinedAt: new Date().toISOString(),
      fullName: 'Amanuel Tesfaye',
      email: 'amanuel@ekub.et',
      avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
      paidThisRound: false
    });

    return of(new HttpResponse({ status: 201, body: newCircle })).pipe(delay(simulatedDelay));
  }

  // 4. POST /api/circles/:id/start
  const startMatch = url.match(/^\/api\/circles\/(\d+)\/start$/);
  if (startMatch && method === 'POST') {
    const circleId = parseInt(startMatch[1], 10);
    const circle = db.circles.find(c => c.id === circleId);
    if (!circle) return makeError(404, 'Not Found', 'Circle not found', 'CIRCLE_NOT_FOUND');

    if (circle.status !== 'Forming') {
      return makeError(400, 'Invalid State', 'Only circles in Forming status can be started.', 'INVALID_STATUS');
    }

    const circleMembers = db.members.filter(m => m.circleId === circleId);
    if (circleMembers.length < 2) {
      return makeError(400, 'Insufficient Members', 'Circle must have at least 2 members before starting.', 'INSUFFICIENT_MEMBERS');
    }

    // NON-NEGOTIABLE RULE: Starting the circle locks member list and fixes payout order
    circle.status = 'Active';
    circle.startedAt = new Date().toISOString();
    circle.currentRoundNumber = 1;

    // Create Round 1 with fixed receiver (Order #1)
    const firstReceiver = circleMembers.sort((a, b) => a.memberOrder - b.memberOrder)[0];
    const newRoundId = db.rounds.length + 1000;
    const firstRound = {
      id: newRoundId,
      circleId,
      roundNumber: 1,
      receiverMemberId: firstReceiver.id,
      receiverName: firstReceiver.fullName,
      receiverAvatar: firstReceiver.avatarUrl,
      status: 'Open' as const,
      potAmount: 0,
      paidMembersCount: 0,
      totalMembersCount: circleMembers.length,
      isEligibleForPayout: false
    };
    db.rounds.push(firstRound);

    return of(new HttpResponse({
      status: 200,
      body: {
        circleId,
        status: 'Active',
        startedAt: circle.startedAt,
        totalRounds: circleMembers.length,
        firstRoundId: newRoundId,
        message: 'Circle successfully started. Member list is locked and payout order is fixed.'
      }
    })).pipe(delay(simulatedDelay));
  }

  // 5. GET /api/circles/:id/members
  const membersMatch = url.match(/^\/api\/circles\/(\d+)\/members$/);
  if (membersMatch && method === 'GET') {
    const circleId = parseInt(membersMatch[1], 10);
    const members = db.members.filter(m => m.circleId === circleId).sort((a, b) => a.memberOrder - b.memberOrder);
    return of(new HttpResponse({ status: 200, body: members })).pipe(delay(simulatedDelay));
  }

  // 6. POST /api/circles/:id/members
  if (membersMatch && method === 'POST') {
    const circleId = parseInt(membersMatch[1], 10);
    const circle = db.circles.find(c => c.id === circleId);
    if (!circle) return makeError(404, 'Not Found', 'Circle not found', 'CIRCLE_NOT_FOUND');

    // NON-NEGOTIABLE RULE: Cannot add members after circle starts
    if (circle.status !== 'Forming') {
      return makeError(400, 'Member List Locked', 'Cannot add new members after circle has started.', 'MEMBER_LIST_LOCKED');
    }

    const body = req.body as any;
    const existing = db.members.filter(m => m.circleId === circleId);
    const newMember = {
      id: db.members.length + 100,
      circleId,
      userId: existing.length + 10,
      memberOrder: existing.length + 1,
      roleInCircle: 'Member' as const,
      hasReceived: false,
      joinedAt: new Date().toISOString(),
      fullName: body.fullName || `Member ${existing.length + 1}`,
      email: body.emailOrPhone || `member${existing.length + 1}@ekub.et`,
      avatarUrl: `https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80`,
      paidThisRound: false
    };
    db.members.push(newMember);
    circle.memberCount = existing.length + 1;
    circle.targetAmount = (circle.memberCount) * circle.contributionAmount;

    return of(new HttpResponse({ status: 201, body: newMember })).pipe(delay(simulatedDelay));
  }

  // 7. GET /api/circles/:id/rounds/current
  const currentRoundMatch = url.match(/^\/api\/circles\/(\d+)\/rounds\/current$/);
  if (currentRoundMatch && method === 'GET') {
    const circleId = parseInt(currentRoundMatch[1], 10);
    const openRound = db.rounds.find(r => r.circleId === circleId && r.status === 'Open');
    if (!openRound) {
      const lastRound = db.rounds.filter(r => r.circleId === circleId).pop();
      return of(new HttpResponse({ status: 200, body: lastRound ?? null })).pipe(delay(simulatedDelay));
    }
    return of(new HttpResponse({ status: 200, body: openRound })).pipe(delay(simulatedDelay));
  }

  // 8. GET /api/circles/:id/rounds
  const roundsMatch = url.match(/^\/api\/circles\/(\d+)\/rounds$/);
  if (roundsMatch && method === 'GET') {
    const circleId = parseInt(roundsMatch[1], 10);
    const rounds = db.rounds.filter(r => r.circleId === circleId).sort((a, b) => b.roundNumber - a.roundNumber);
    return of(new HttpResponse({ status: 200, body: rounds })).pipe(delay(simulatedDelay));
  }

  // 9. POST /api/rounds/:id/payments
  const paymentMatch = url.match(/^\/api\/rounds\/(\d+)\/payments$/);
  if (paymentMatch && method === 'POST') {
    const roundId = parseInt(paymentMatch[1], 10);
    const body = req.body as any;
    const memberId = Number(body.circleMemberId);

    const round = db.rounds.find(r => r.id === roundId);
    if (!round) return makeError(404, 'Not Found', 'Round not found', 'ROUND_NOT_FOUND');

    if (round.status !== 'Open') {
      return makeError(400, 'Round Closed', 'Payments cannot be recorded for a closed or paid-out round.', 'ROUND_CLOSED');
    }

    // NON-NEGOTIABLE RULE: Duplicate normal payment must be rejected by API
    const duplicate = db.payments.find(p => p.roundId === roundId && p.circleMemberId === memberId && p.paymentType === 'Normal');
    if (duplicate) {
      return makeError(409, 'Duplicate Payment', 'Duplicate payment rejected: Member has already contributed for this round.', 'DUPLICATE_PAYMENT');
    }

    const member = db.members.find(m => m.id === memberId);
    const circle = db.circles.find(c => c.id === round.circleId);
    const amount = circle ? circle.contributionAmount : 1000;

    const newPayment = {
      id: db.payments.length + 5000,
      roundId,
      circleMemberId: memberId,
      amount,
      paymentType: 'Normal' as const,
      status: 'Completed' as const,
      paidAt: new Date().toISOString(),
      memberName: member?.fullName || 'Circle Member',
      circleName: circle?.name || 'Ekub Circle',
      roundNumber: round.roundNumber
    };
    db.payments.unshift(newPayment);

    // Update Round Pot & Paid Count
    round.paidMembersCount = (round.paidMembersCount ?? 0) + 1;
    round.potAmount = (round.potAmount ?? 0) + amount;
    if (round.paidMembersCount >= (round.totalMembersCount ?? 1)) {
      round.isEligibleForPayout = true;
    }

    // Mark member paid
    if (member) member.paidThisRound = true;
    if (circle) circle.totalSaved = (circle.totalSaved ?? 0) + amount;

    return of(new HttpResponse({ status: 201, body: newPayment })).pipe(delay(simulatedDelay));
  }

  // 10. POST /api/rounds/:id/payout
  const payoutMatch = url.match(/^\/api\/rounds\/(\d+)\/payout$/);
  if (payoutMatch && method === 'POST') {
    const roundId = parseInt(payoutMatch[1], 10);
    const round = db.rounds.find(r => r.id === roundId);
    if (!round) return makeError(404, 'Not Found', 'Round not found', 'ROUND_NOT_FOUND');

    // NON-NEGOTIABLE RULE: A paid-out round cannot be paid out again
    if (round.status === 'Paid Out') {
      return makeError(400, 'Already Paid Out', 'This round has already been paid out and cannot be paid again.', 'ALREADY_PAID_OUT');
    }

    // NON-NEGOTIABLE RULE: The current round cannot be paid out until all required members have paid
    if ((round.paidMembersCount ?? 0) < (round.totalMembersCount ?? 1)) {
      return makeError(
        422,
        'Payout Blocked',
        `Cannot disburse pot. Only ${round.paidMembersCount} of ${round.totalMembersCount} members have contributed.`,
        'MEMBERS_NOT_ALL_PAID'
      );
    }

    // Mark Round Paid Out
    round.status = 'Paid Out';
    round.paidOutAt = new Date().toISOString();
    round.isEligibleForPayout = false;

    // Mark receiver hasReceived = true
    const receiver = db.members.find(m => m.id === round.receiverMemberId);
    if (receiver) receiver.hasReceived = true;

    const circle = db.circles.find(c => c.id === round.circleId);
    const circleMembers = db.members.filter(m => m.circleId === round.circleId).sort((a, b) => a.memberOrder - b.memberOrder);

    // Check if there is a next round
    const nextReceiver = circleMembers.find(m => !m.hasReceived);
    let nextRound: any = null;
    let isCompleted = false;

    if (nextReceiver) {
      // NON-NEGOTIABLE RULE: Previous receiver continues paying in later rounds!
      circleMembers.forEach(m => m.paidThisRound = false);

      const nextRoundNumber = round.roundNumber + 1;
      nextRound = {
        id: db.rounds.length + 1000,
        circleId: round.circleId,
        roundNumber: nextRoundNumber,
        receiverMemberId: nextReceiver.id,
        receiverName: nextReceiver.fullName,
        receiverAvatar: nextReceiver.avatarUrl,
        status: 'Open' as const,
        potAmount: 0,
        paidMembersCount: 0,
        totalMembersCount: circleMembers.length,
        isEligibleForPayout: false
      };
      db.rounds.push(nextRound);
      if (circle) circle.currentRoundNumber = nextRoundNumber;
    } else {
      // All members have received! Circle is Completed
      isCompleted = true;
      if (circle) {
        circle.status = 'Completed';
        circle.completedAt = new Date().toISOString();
      }
    }

    return of(new HttpResponse({
      status: 200,
      body: {
        roundId,
        circleId: round.circleId,
        roundNumber: round.roundNumber,
        receiverMemberId: round.receiverMemberId,
        receiverName: round.receiverName,
        potAmount: round.potAmount,
        status: 'Paid Out',
        paidOutAt: round.paidOutAt,
        nextRound,
        isCircleCompleted: isCompleted,
        message: isCompleted 
          ? `All ${circleMembers.length} rounds complete! Circle has completed successfully.`
          : `Payout disbursed to ${round.receiverName}. Round ${round.roundNumber + 1} is now Open.`
      }
    })).pipe(delay(simulatedDelay));
  }

  // 11. GET /api/circles/:id/history
  const historyMatch = url.match(/^\/api\/circles\/(\d+)\/history$/);
  if (historyMatch && method === 'GET') {
    const circleId = parseInt(historyMatch[1], 10);
    const circlePayments = db.payments.filter(p => {
      const r = db.rounds.find(round => round.id === p.roundId);
      return r && r.circleId === circleId;
    });
    return of(new HttpResponse({ status: 200, body: circlePayments })).pipe(delay(simulatedDelay));
  }

  // 12. GET /api/notifications
  if (url === '/api/notifications' && method === 'GET') {
    return of(new HttpResponse({ status: 200, body: db.notifications })).pipe(delay(simulatedDelay));
  }

  // Default passthrough
  return next(req);
};
