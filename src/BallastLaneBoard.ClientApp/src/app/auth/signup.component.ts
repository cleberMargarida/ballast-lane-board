import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { ToastService } from '../shared/toast.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="signup-wrapper">
      <div class="signup-card">
        <h1 class="signup-title">Sign up</h1>
        <p class="signup-subtitle">Free access to our dashboard.</p>

        <form (ngSubmit)="onSubmit()">
          <div class="field">
            <label for="username" class="field-label">Username</label>
            <input id="username" [(ngModel)]="username" name="username" required
                   class="field-input" placeholder="Choose a username" />
          </div>

          <div class="field">
            <label for="email" class="field-label">Email Address</label>
            <input id="email" type="email" [(ngModel)]="email" name="email" required
                   class="field-input" placeholder="name@address.com" />
          </div>

          <div class="field">
            <label for="password" class="field-label">Password</label>
            <input id="password" type="password" [(ngModel)]="password" name="password" required
                   class="field-input" placeholder="Enter your password" />
          </div>

          @if (error) {
            <div class="form-error">{{ error }}</div>
          }

          <button type="submit" class="btn-signup" [disabled]="loading">
            {{ loading ? 'Signing up…' : 'Sign up' }}
          </button>
        </form>

        <p class="signup-footer">
          Already have an account? <a (click)="auth.login()" class="signup-link">Log in</a>.
        </p>
      </div>
    </div>
  `,
  styles: [`
    .signup-wrapper {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: calc(100vh - 56px);
      padding: 2rem 1rem;
    }
    .signup-card {
      background: #fff;
      border: 1px solid #e3ebf6;
      border-radius: 0.5rem;
      box-shadow: 0 1px 3px rgba(18, 38, 63, 0.06);
      padding: 2rem;
      width: 100%;
      max-width: 540px;
    }
    .signup-title {
      font-size: 1.5rem;
      font-weight: 700;
      color: #12263f;
      margin: 0 0 0.375rem;
      text-align: center;
    }
    .signup-subtitle {
      font-size: 0.9375rem;
      color: #95aac9;
      margin: 0 0 1.5rem;
      text-align: center;
    }
    .field { margin-bottom: 1rem; }
    .field-label {
      display: block;
      font-size: 0.8125rem;
      font-weight: 500;
      color: #12263f;
      margin-bottom: 0.375rem;
    }
    .field-input {
      width: 100%;
      background: #fff;
      border: 1px solid #d2ddec;
      border-radius: 0.375rem;
      padding: 0.5rem 0.75rem;
      font-size: 0.9375rem;
      color: #12263f;
      transition: border-color 150ms, box-shadow 150ms;
      box-sizing: border-box;
      font-family: inherit;
    }
    .field-input::placeholder { color: #b1c2d9; }
    .field-input:focus {
      outline: none;
      border-color: #2c7be5;
      box-shadow: 0 0 0 3px rgba(44, 123, 229, 0.15);
    }
    .form-error {
      background: #fde8e8;
      border: 1px solid #f5c6cb;
      color: #e63757;
      border-radius: 0.375rem;
      padding: 0.5rem 0.75rem;
      font-size: 0.8125rem;
      margin-bottom: 1rem;
    }
    .btn-signup {
      display: block;
      width: 100%;
      background: #2c7be5;
      color: #fff;
      border: none;
      padding: 0.625rem 1.25rem;
      border-radius: 0.375rem;
      font-size: 0.9375rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 150ms;
      margin-bottom: 1.5rem;
    }
    .btn-signup:hover { background: #1a68d1; }
    .btn-signup:disabled { opacity: 0.65; cursor: not-allowed; }
    .signup-footer {
      text-align: center;
      font-size: 0.8125rem;
      color: #95aac9;
      margin: 0;
    }
    .signup-link {
      color: #2c7be5;
      cursor: pointer;
      text-decoration: none;
      font-weight: 500;
    }
    .signup-link:hover { text-decoration: underline; }
  `],
})
export class SignupComponent {
  username = '';
  email = '';
  password = '';
  error: string | null = null;
  loading = false;

  constructor(
    private api: ApiService,
    public auth: AuthService,
    private toast: ToastService,
    private router: Router,
  ) {}

  onSubmit(): void {
    this.error = null;
    this.loading = true;
    this.api.register(this.username, this.email, this.password).subscribe({
      next: () => {
        this.toast.success('Account created! Please sign in.');
        this.auth.login();
      },
      error: (err) => {
        this.loading = false;
        const body = err?.error;
        if (typeof body === 'string' && body.length > 0) {
          this.error = body;
        } else if (typeof body === 'object' && body?.message) {
          this.error = body.message;
        } else {
          this.error = 'Registration failed. Please try again.';
        }
      },
    });
  }
}
