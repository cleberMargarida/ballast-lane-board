import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { Toast, ToastService } from './toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      @for (toast of toasts; track toast.id) {
        <div [class]="'toast toast-' + toast.type" (click)="remove(toast.id)">
          <span class="toast-icon">
            @switch (toast.type) {
              @case ('success') { ✓ }
              @case ('error') { ✕ }
              @case ('info') { ℹ }
            }
          </span>
          <span class="toast-message">{{ toast.message }}</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 1rem;
      right: 1rem;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      max-width: 380px;
    }
    .toast {
      display: flex;
      align-items: center;
      gap: 0.625rem;
      padding: 0.75rem 1rem;
      border-radius: 0.375rem;
      font-size: 0.875rem;
      font-weight: 500;
      color: #fff;
      box-shadow: 0 4px 16px rgba(18, 38, 63, 0.2);
      cursor: pointer;
      animation: toastIn 200ms ease;
    }
    .toast-success { background: #00d97e; }
    .toast-error   { background: #e63757; }
    .toast-info    { background: #2c7be5; }
    .toast-icon {
      font-size: 1rem;
      font-weight: 700;
      flex-shrink: 0;
    }
    .toast-message { line-height: 1.4; }
    @keyframes toastIn {
      from { opacity: 0; transform: translateX(20px); }
      to   { opacity: 1; transform: translateX(0); }
    }
  `],
})
export class ToastComponent implements OnInit, OnDestroy {
  toasts: { id: number; message: string; type: string }[] = [];
  private sub!: Subscription;
  private nextId = 0;

  constructor(private toastService: ToastService) {}

  ngOnInit(): void {
    this.sub = this.toastService.toast$.subscribe((toast: Toast) => {
      const id = this.nextId++;
      this.toasts.push({ id, ...toast });
      setTimeout(() => this.remove(id), 4000);
    });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  remove(id: number): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }
}
