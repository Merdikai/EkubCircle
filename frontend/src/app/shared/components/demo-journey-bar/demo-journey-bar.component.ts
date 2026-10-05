import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService, DEMO_USERS } from '../../../core/services';

interface DemoStep {
  num: number;
  label: string;
  route: string;
  persona: 'Organizer' | 'Member';
  desc: string;
}

@Component({
  selector: 'app-demo-journey-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './demo-journey-bar.component.html',
  styleUrls: ['./demo-journey-bar.component.css']
})
export class DemoJourneyBarComponent {
  private router = inject(Router);
  authService = inject(AuthService);

  isExpanded = signal<boolean>(false);
  currentStepIndex = signal<number>(0);

  demoSteps: DemoStep[] = [
    { num: 1, label: '1. Dashboard Overview', route: '/dashboard', persona: 'Organizer', desc: 'Inspect metric cards, Ethiopian banknote hero banner & groups' },
    { num: 2, label: '2. Create Ekub Circle', route: '/circles/create', persona: 'Organizer', desc: 'Step wizard with fixed contribution & duration' },
    { num: 3, label: '3. Forming Circle (Add Members)', route: '/circles/4/members', persona: 'Organizer', desc: 'Add members before start; check join requests' },
    { num: 4, label: '4. Start Circle Confirmation', route: '/circles/4', persona: 'Organizer', desc: 'Lock member list & generate fixed server payout order' },
    { num: 5, label: '5. Active Round 2 Status', route: '/circles/1/round', persona: 'Organizer', desc: 'Inspect pot, paid/unpaid members & fixed receiver' },
    { num: 6, label: '6. Duplicate Payment Test', route: '/circles/1/round', persona: 'Organizer', desc: 'Record payment & verify API duplicate rejection' },
    { num: 7, label: '7. Early Payout Block Test', route: '/circles/1/round', persona: 'Organizer', desc: 'Verify payout is locked until all 12 members contribute' },
    { num: 8, label: '8. Complete Round Payout', route: '/circles/1/round', persona: 'Organizer', desc: 'Confirm pot handover to server-fixed recipient' },
    { num: 9, label: '9. Winner Continuing Payment', route: '/circles/1/history', persona: 'Organizer', desc: 'Verify Round 1 winner continues contributing in Round 2' },
    { num: 10, label: '10. Completed Circle State', route: '/circles/5', persona: 'Organizer', desc: 'Inspect finished Ekub with all rounds completed' }
  ];

  toggleExpand(): void {
    this.isExpanded.update(v => !v);
  }

  jumpToStep(index: number): void {
    this.currentStepIndex.set(index);
    const step = this.demoSteps[index];

    // Ensure persona matches step
    if (step.persona === 'Organizer' && this.authService.currentUser()?.role !== 'Organizer') {
      this.authService.loginWithDemoUser(DEMO_USERS[0]);
    } else if (step.persona === 'Member' && this.authService.currentUser()?.role !== 'Member') {
      this.authService.loginWithDemoUser(DEMO_USERS[1]);
    }

    this.router.navigateByUrl(step.route);
  }
}
