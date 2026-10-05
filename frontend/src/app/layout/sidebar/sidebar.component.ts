import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  exact?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  authService = inject(AuthService);

  navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'dashboard', exact: true },
    { label: 'My Groups', route: '/circles', icon: 'groups' },
    { label: 'Create Group', route: '/circles/create', icon: 'add_circle' },
    { label: 'Contributions', route: '/circles/1/round', icon: 'payments' },
    { label: 'Payouts', route: '/circles/1/history', icon: 'redeem' },
    { label: 'Transactions', route: '/wallet', icon: 'receipt_long' },
    { label: 'Profile', route: '/profile', icon: 'account_circle' },
    { label: 'Settings', route: '/settings', icon: 'settings' }
  ];
}
