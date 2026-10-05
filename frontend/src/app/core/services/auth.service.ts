import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { User, AuthSession, LoginRequest } from '../models';

const STORAGE_KEY = 'ekub_auth_session';

export const DEMO_USERS: User[] = [
  {
    id: 1,
    fullName: 'Abebe Bikila',
    email: 'organizer@ekub.local',
    role: 'Organizer',
    phoneNumber: '+251 911 234 567',
    avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
    walletBalance: 12500,
    createdAt: '2026-01-01T08:00:00Z'
  },
  {
    id: 2,
    fullName: 'Hana Gebre',
    email: 'member1@ekub.local',
    role: 'Member',
    phoneNumber: '+251 922 345 678',
    avatarUrl: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&auto=format&fit=crop&q=80',
    walletBalance: 4500,
    createdAt: '2026-01-01T09:30:00Z'
  },
  {
    id: 3,
    fullName: 'Dawit Alemu',
    email: 'member2@ekub.local',
    role: 'Member',
    phoneNumber: '+251 933 456 789',
    avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80',
    walletBalance: 5000,
    createdAt: '2026-01-01T11:00:00Z'
  },
  {
    id: 99,
    fullName: 'Hackathon Admin',
    email: 'admin@hackathon.local',
    role: 'Admin',
    phoneNumber: '+251 999 000 111',
    avatarUrl: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&auto=format&fit=crop&q=80',
    walletBalance: 99999,
    createdAt: '2026-01-01T00:00:00Z'
  }
];

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly sessionSignal = signal<AuthSession | null>(this.loadStoredSession());

  // Public Signals
  readonly session = this.sessionSignal.asReadonly();
  readonly currentUser = computed(() => this.sessionSignal()?.user ?? null);
  readonly isAuthenticated = computed(() => !!this.sessionSignal());
  readonly isOrganizer = computed(() => {
    const role = this.currentUser()?.role;
    return role === 'Organizer' || role === 'Admin';
  });

  constructor() {
    // If no session exists, default to organizer for immediate live presentation
    if (!this.sessionSignal()) {
      this.loginWithDemoUser(DEMO_USERS[0]);
    }
  }

  loginApi(credentials: LoginRequest): Observable<AuthSession> {
    return this.http.post<AuthSession>('/api/auth/login', credentials).pipe(
      tap(session => {
        this.sessionSignal.set(session);
        try {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        } catch {}
      })
    );
  }

  registerApi(data: { fullName: string; email: string; password: string; role: string }): Observable<AuthSession> {
    return this.http.post<AuthSession>('/api/auth/register', data).pipe(
      tap(session => {
        this.sessionSignal.set(session);
        try {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        } catch {}
      })
    );
  }

  login(credentials: LoginRequest): boolean {
    const foundUser = DEMO_USERS.find(
      u => u.email.toLowerCase() === credentials.email.toLowerCase()
    ) || DEMO_USERS[0];

    return this.loginWithDemoUser(foundUser);
  }

  loginWithDemoUser(user: User): boolean {
    const session: AuthSession = {
      user,
      token: `demo-jwt-token-ekub-${user.id}-${Date.now()}`,
      expiresAt: new Date(Date.now() + 86400000).toISOString()
    };

    this.sessionSignal.set(session);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } catch {}
    return true;
  }

  logout(): void {
    this.sessionSignal.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {}
    this.router.navigate(['/login']);
  }

  updateWalletBalance(delta: number): void {
    const current = this.sessionSignal();
    if (!current) return;

    const newBalance = Math.max(0, (current.user.walletBalance ?? 0) + delta);
    const updatedUser: User = { ...current.user, walletBalance: newBalance };
    const updatedSession: AuthSession = { ...current, user: updatedUser };

    this.sessionSignal.set(updatedSession);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(updatedSession));
    } catch {}
  }

  private loadStoredSession(): AuthSession | null {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        return JSON.parse(stored) as AuthSession;
      }
    } catch {
      return null;
    }
    return null;
  }
}
