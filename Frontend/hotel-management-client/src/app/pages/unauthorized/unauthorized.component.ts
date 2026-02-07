import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-unauthorized',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="unauthorized-container">
      <div class="unauthorized-card">
        <div class="icon">🚫</div>
        <h1>Access Denied</h1>
        <p>You don't have permission to access this page.</p>
        <p class="subtitle">Please contact your administrator if you believe this is an error.</p>
        <a routerLink="/login" class="btn-primary">Return to Login</a>
      </div>
    </div>
  `,
    styles: [`
    .unauthorized-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .unauthorized-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
      padding: 48px 40px;
      text-align: center;
      max-width: 480px;
    }

    .icon {
      font-size: 4rem;
      margin-bottom: 16px;
    }

    h1 {
      font-size: 1.875rem;
      font-weight: 700;
      color: #1f2937;
      margin: 0 0 12px 0;
    }

    p {
      color: #6b7280;
      font-size: 1rem;
      margin: 0 0 8px 0;
    }

    .subtitle {
      font-size: 0.875rem;
      margin-bottom: 24px;
    }

    .btn-primary {
      display: inline-block;
      padding: 12px 32px;
      background: #3b82f6;
      color: white;
      text-decoration: none;
      border-radius: 8px;
      font-weight: 600;
      transition: all 0.2s;
    }

    .btn-primary:hover {
      background: #2563eb;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
    }
  `]
})
export class UnauthorizedComponent { }
