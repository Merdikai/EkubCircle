import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService, DEMO_USERS } from '../../../core/services';
import { User, UserRole } from '../../../core/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  authMode = signal<'login' | 'register'>('login');
  loginForm: FormGroup;
  registerForm: FormGroup;
  showPassword = signal(false);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  demoUsers = DEMO_USERS;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['organizer@ekub.local', [Validators.required, Validators.email]],
      password: ['Ekub123!', [Validators.required, Validators.minLength(6)]]
    });

    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['+251911000000', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Member', [Validators.required]]
    });
  }

  setMode(mode: 'login' | 'register'): void {
    this.authMode.set(mode);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.loginForm.value;

    this.authService.loginApi({ email, password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        // Fallback to local demo user check if offline or mock account
        const fallbackSuccess = this.authService.login({ email, password });
        this.isLoading.set(false);
        if (fallbackSuccess) {
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
          this.router.navigateByUrl(returnUrl);
        } else {
          const detail = err.error?.message || err.error?.detail || 'Invalid email or password. Please check your credentials.';
          this.errorMessage.set(detail);
        }
      }
    });
  }

  onRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const formVal = this.registerForm.value;

    this.authService.registerApi({
      fullName: formVal.fullName,
      email: formVal.email,
      phoneNumber: formVal.phoneNumber,
      password: formVal.password,
      role: formVal.role
    }).subscribe({
      next: () => {
        this.isLoading.set(false);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.isLoading.set(false);
        const detail = err.error?.message || err.error?.detail || 'Failed to register account. Please try again.';
        this.errorMessage.set(detail);
      }
    });
  }

  selectDemoUser(user: User): void {
    const password = user.role === 'Admin' ? 'Admin123!' : 'Ekub123!';
    this.authMode.set('login');
    this.loginForm.patchValue({
      email: user.email,
      password: password
    });
    this.onLogin();
  }
}
