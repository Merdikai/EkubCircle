import { Routes } from '@angular/router';
import { authGuard } from './core/guards';
import { AppShellComponent } from './layout';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'circles',
        loadComponent: () => import('./features/circles/circles-list/circles-list.component').then(m => m.CirclesListComponent)
      },
      {
        path: 'circles/create',
        loadComponent: () => import('./features/circles/create-circle/create-circle.component').then(m => m.CreateCircleComponent)
      },
      {
        path: 'circles/:circleId',
        loadComponent: () => import('./features/circles/circle-overview/circle-overview.component').then(m => m.CircleOverviewComponent)
      },
      {
        path: 'circles/:circleId/members',
        loadComponent: () => import('./features/members/members.component').then(m => m.MembersComponent)
      },
      {
        path: 'circles/:circleId/round',
        loadComponent: () => import('./features/rounds/round-status/round-status.component').then(m => m.RoundStatusComponent)
      },
      {
        path: 'circles/:circleId/history',
        loadComponent: () => import('./features/rounds/round-history/round-history.component').then(m => m.RoundHistoryComponent)
      },
      {
        path: 'wallet',
        loadComponent: () => import('./features/payments/wallet-transactions/wallet-transactions.component').then(m => m.WalletTransactionsComponent)
      },
      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notifications.component').then(m => m.NotificationsComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
