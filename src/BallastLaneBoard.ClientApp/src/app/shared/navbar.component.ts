import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <nav class="navbar">
      <div class="navbar-inner">
        <a routerLink="/" class="navbar-brand">
          <svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 24 24" fill="none"
               stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>
            <rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/>
          </svg>
          Ballast Lane Board
        </a>

        <div class="navbar-actions">
          @if (auth.isLoggedIn) {
            <span class="navbar-user">
              {{ auth.username }}
              @if (auth.isAdmin) {
                <span class="navbar-badge">Admin</span>
              }
            </span>
            <button (click)="auth.logout()" class="navbar-logout">Logout</button>
          } @else {
            <a routerLink="/signup" class="navbar-signup">Sign Up</a>
            <button (click)="auth.login()" class="navbar-login">Sign In</button>
          }
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      background: #fff;
      border-bottom: 1px solid #e3ebf6;
    }
    .navbar-inner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 2rem;
    }
    .navbar-brand {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.0625rem;
      font-weight: 700;
      color: #12263f;
      text-decoration: none;
      transition: color 150ms;
    }
    .navbar-brand svg { color: #2c7be5; }
    .navbar-brand:hover { color: #2c7be5; }
    .navbar-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .navbar-user {
      font-size: 0.8125rem;
      color: #6e84a3;
    }
    .navbar-badge {
      font-size: 0.6875rem;
      background: #e0d4f5;
      color: #6b58b8;
      padding: 0.1rem 0.4rem;
      border-radius: 0.25rem;
      margin-left: 0.25rem;
      font-weight: 600;
    }
    .navbar-logout {
      font-size: 0.8125rem;
      color: #e63757;
      background: transparent;
      border: none;
      cursor: pointer;
      transition: color 150ms;
    }
    .navbar-logout:hover { color: #c62a47; }
    .navbar-login {
      font-size: 0.8125rem;
      font-weight: 500;
      color: #fff;
      background: #2c7be5;
      border: none;
      padding: 0.4rem 1rem;
      border-radius: 0.375rem;
      cursor: pointer;
      transition: background 150ms;
    }
    .navbar-login:hover { background: #1a68d1; }
    .navbar-signup {
      font-size: 0.8125rem;
      font-weight: 500;
      color: #2c7be5;
      text-decoration: none;
      transition: color 150ms;
    }
    .navbar-signup:hover { color: #1a68d1; }
  `],
})
export class NavbarComponent {
  constructor(public auth: AuthService) {}
}
