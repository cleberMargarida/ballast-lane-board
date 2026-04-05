import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { CreateTaskRequest, UpdateTaskRequest } from './task.model';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="modal-wrapper">
      <div class="modal-panel">

        <!-- Header row -->
        <div class="modal-header-row">
          <div class="modal-header-content">
            <h6 class="modal-pretitle">
              <a routerLink="/tasks">Task Board</a>
            </h6>
            <h2 class="modal-title">{{ isEdit ? 'Edit Task' : 'Create a New Task' }}</h2>
            <p class="modal-subtitle">
              {{ isEdit ? 'Update the details of your task below.' : 'Fill in the details below to add a new card to your board.' }}
            </p>
          </div>
          <a routerLink="/tasks" class="modal-close" aria-label="Close">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </a>
        </div>

        <!-- Divider -->
        <hr class="modal-divider">

        <!-- Details card -->
        <form (ngSubmit)="save()">
          <div class="inner-card">
            <div class="inner-card-header">
              <h4 class="inner-card-title">Details</h4>
            </div>
            <div class="inner-card-body">

              <div class="field">
                <label for="title" class="field-label">Title *</label>
                <input id="title" [(ngModel)]="title" name="title" required
                       class="field-input" placeholder="Enter a task title" />
              </div>

              <div class="field">
                <label for="description" class="field-label">Description</label>
                <textarea id="description" [(ngModel)]="description" name="description" rows="4"
                          class="field-input field-textarea"
                          placeholder="Add a more detailed description…"></textarea>
              </div>

              <div class="field">
                <label for="dueDate" class="field-label">Due Date</label>
                <input id="dueDate" type="date" [(ngModel)]="dueDate" name="dueDate" class="field-input" />
              </div>

            </div>
          </div>

          @if (error) {
            <div class="form-error">{{ error }}</div>
          }

          <!-- Action buttons -->
          <div class="modal-actions">
            <button type="submit" class="btn-primary">
              {{ isEdit ? 'Save Changes' : 'Create Task' }}
            </button>
            <a routerLink="/tasks" class="btn-white">Cancel</a>
          </div>
        </form>

      </div>
    </div>
  `,
  styles: [`
    /* ── Modal wrapper ── */
    .modal-wrapper {
      max-width: 600px;
      margin: 0 auto;
    }
    .modal-panel {
      background: #fff;
      border: 1px solid #e3ebf6;
      border-radius: 0.5rem;
      box-shadow: 0 1px 3px rgba(18, 38, 63, 0.06);
      padding: 2rem;
    }

    /* ── Header ── */
    .modal-header-row {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
    }
    .modal-header-content { flex: 1; }
    .modal-pretitle {
      font-size: 0.6875rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #95aac9;
      margin: 0 0 0.5rem;
    }
    .modal-pretitle a {
      color: #95aac9;
      text-decoration: none;
      transition: color 150ms;
    }
    .modal-pretitle a:hover { color: #2c7be5; }
    .modal-title {
      font-size: 1.5rem;
      font-weight: 700;
      color: #12263f;
      margin: 0 0 0.375rem;
    }
    .modal-subtitle {
      font-size: 0.9375rem;
      color: #95aac9;
      margin: 0;
      line-height: 1.5;
    }
    .modal-close {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 2rem;
      height: 2rem;
      border-radius: 0.25rem;
      color: #95aac9;
      text-decoration: none;
      transition: background 100ms, color 100ms;
      flex-shrink: 0;
    }
    .modal-close:hover { background: #edf2f9; color: #12263f; }

    /* ── Divider ── */
    .modal-divider {
      border: none;
      border-top: 1px solid #e3ebf6;
      margin: 1.5rem 0;
    }

    /* ── Inner card ── */
    .inner-card {
      background: #fff;
      border: 1px solid #e3ebf6;
      border-radius: 0.5rem;
      margin-bottom: 1.25rem;
    }
    .inner-card-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.875rem 1.25rem;
      border-bottom: 1px solid #e3ebf6;
    }
    .inner-card-title {
      font-size: 0.9375rem;
      font-weight: 600;
      color: #12263f;
      margin: 0;
    }
    .inner-card-body { padding: 1.25rem; }

    /* ── Fields ── */
    .field { margin-bottom: 1rem; }
    .field:last-child { margin-bottom: 0; }
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
    .field-textarea { resize: vertical; min-height: 100px; }

    /* ── Error ── */
    .form-error {
      background: #fde8e8;
      border: 1px solid #f5c6cb;
      color: #e63757;
      border-radius: 0.375rem;
      padding: 0.5rem 0.75rem;
      font-size: 0.8125rem;
      margin-bottom: 1.25rem;
    }

    /* ── Buttons ── */
    .modal-actions {
      display: flex;
      gap: 0.75rem;
    }
    .btn-primary {
      background: #2c7be5;
      color: #fff;
      border: none;
      padding: 0.5rem 1.25rem;
      border-radius: 0.375rem;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 150ms;
    }
    .btn-primary:hover { background: #1a68d1; }
    .btn-white {
      background: #fff;
      color: #12263f;
      border: 1px solid #d2ddec;
      padding: 0.5rem 1.25rem;
      border-radius: 0.375rem;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      text-decoration: none;
      transition: background 100ms, border-color 100ms;
      display: inline-flex;
      align-items: center;
    }
    .btn-white:hover { background: #f9fbfd; border-color: #c6d3e6; }
  `],
})
export class TaskFormComponent implements OnInit {
  title = '';
  description = '';
  dueDate = '';
  error: string | null = null;
  isEdit = false;
  private taskId: string | null = null;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get('id');
    if (this.taskId) {
      this.isEdit = true;
      this.api.getTask(this.taskId).subscribe(task => {
        this.title = task.title;
        this.description = task.description ?? '';
        this.dueDate = task.dueDate ? task.dueDate.substring(0, 10) : '';
      });
    }
  }

  save(): void {
    this.error = null;

    if (this.isEdit && this.taskId) {
      const request: UpdateTaskRequest = {
        title: this.title,
        description: this.description || undefined,
        dueDate: this.dueDate ? this.dueDate + 'T00:00:00.000Z' : undefined,
      };
      this.api.updateTask(this.taskId, request).subscribe({
        next: () => this.router.navigate(['/tasks']),
        error: err => (this.error = err.error?.toString() ?? 'Update failed'),
      });
    } else {
      const request: CreateTaskRequest = {
        title: this.title,
        description: this.description || undefined,
        dueDate: this.dueDate ? this.dueDate + 'T00:00:00.000Z' : undefined,
      };
      this.api.createTask(request).subscribe({
        next: () => this.router.navigate(['/tasks']),
        error: err => (this.error = err.error?.toString() ?? 'Create failed'),
      });
    }
  }
}
