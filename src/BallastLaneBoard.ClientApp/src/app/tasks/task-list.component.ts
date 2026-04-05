import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CdkDragDrop,
  CdkDrag,
  CdkDropList,
  CdkDropListGroup,
  moveItemInArray,
  transferArrayItem,
} from '@angular/cdk/drag-drop';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { ToastService } from '../shared/toast.service';
import { TaskResponse, TaskItemStatus, CreateTaskRequest, UpdateTaskRequest } from './task.model';

interface KanbanColumn {
  status: TaskItemStatus;
  label: string;
  tasks: TaskResponse[];
}

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, CdkDropListGroup, CdkDropList, CdkDrag],
  template: `
    <!-- Header -->
    <div class="kanban-header">
      <div>
        <span class="kanban-breadcrumb">TASK BOARD</span>
        <h1 class="kanban-title">Ballast Lane Board</h1>
      </div>
      <button (click)="openNewTask()" class="kanban-add-btn">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        New Task
      </button>
    </div>

    <!-- Kanban Board -->
    <div class="kanban-board" cdkDropListGroup>
      @for (col of columns; track col.status) {
        <div class="kanban-column">

          <!-- Column Header -->
          <div class="kanban-column-header">
            <h4 class="kanban-column-title">{{ col.label }}</h4>
            <span class="kanban-column-count">{{ col.tasks.length }}</span>
          </div>

          <!-- Drop Zone -->
          <div cdkDropList
               [cdkDropListData]="col.tasks"
               (cdkDropListDropped)="drop($event, col.status)"
               class="kanban-drop-zone">

            @for (task of col.tasks; track task.id) {
              <div cdkDrag class="kanban-card" (click)="openEditTask(task)">
                <!-- Drag handle preview -->
                <div *cdkDragPlaceholder class="kanban-card-placeholder"></div>

                <!-- Status badge -->
                <div class="kanban-card-badges">
                  <span [class]="'kanban-badge ' + badgeClass(task.status)">
                    {{ statusLabel(task.status) }}
                  </span>
                </div>

                <!-- Title & description -->
                <p class="kanban-card-title">{{ task.title }}</p>
                @if (task.description) {
                  <p class="kanban-card-desc">{{ task.description }}</p>
                }

                <!-- Footer: due date & actions -->
                <div class="kanban-card-footer">
                  <div class="kanban-card-meta">
                    @if (task.dueDate) {
                      <span class="kanban-card-date">
                        <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24"
                             fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                        </svg>
                        {{ task.dueDate | date:'MMM d' }}
                      </span>
                    }
                  </div>
                  <div class="kanban-card-actions">
                    <button (click)="openEditTask(task); $event.stopPropagation()" class="kanban-action-btn" title="Edit">
                      <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                        <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                      </svg>
                    </button>
                    <button (click)="deleteTask(task); $event.stopPropagation()" class="kanban-action-btn kanban-action-delete" title="Delete">
                      <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                           fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
                        <path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/>
                      </svg>
                    </button>
                  </div>
                </div>
              </div>
            } @empty {
              <div class="kanban-empty">
                <p>No tasks</p>
              </div>
            }
          </div>

          <!-- Add card link -->
          <button (click)="openNewTask()" class="kanban-add-card">+ Add Card</button>
        </div>
      }
    </div>

    <!-- ═══ Modal Overlay ═══ -->
    @if (modalOpen) {
      <div class="modal-backdrop" (click)="closeModal()"></div>
      <div class="modal-dialog" role="dialog">
        <div class="modal-panel">

          <!-- Header -->
          <div class="modal-header-row">
            <div class="modal-header-content">
              <h6 class="modal-pretitle">
                {{ editingTask ? statusLabel(editingTask.status) : 'NEW TASK' }}
              </h6>
              <h2 class="modal-title">{{ editingTask ? 'Edit Task' : 'Create a New Task' }}</h2>
              <p class="modal-subtitle">
                {{ editingTask ? 'Update the details of your task below.' : 'Fill in the details below to add a new card to your board.' }}
              </p>
            </div>
            <button (click)="closeModal()" class="modal-close" aria-label="Close">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          </div>

          <hr class="modal-divider">

          <!-- Form -->
          <form (ngSubmit)="saveModal()">
            <div class="inner-card">
              <div class="inner-card-header">
                <h4 class="inner-card-title">Details</h4>
              </div>
              <div class="inner-card-body">
                <div class="field">
                  <label for="m-title" class="field-label">Title *</label>
                  <input id="m-title" [(ngModel)]="formTitle" name="title" required
                         class="field-input" placeholder="Enter a task title" />
                </div>
                <div class="field">
                  <label for="m-desc" class="field-label">Description</label>
                  <textarea id="m-desc" [(ngModel)]="formDescription" name="description" rows="4"
                            class="field-input field-textarea"
                            placeholder="Add a more detailed description…"></textarea>
                </div>
                <div class="field">
                  <label for="m-due" class="field-label">Due Date</label>
                  <input id="m-due" type="date" [(ngModel)]="formDueDate" name="dueDate"
                         class="field-input" />
                </div>
              </div>
            </div>

            @if (formError) {
              <div class="form-error">{{ formError }}</div>
            }

            <div class="modal-actions">
              <button type="submit" class="btn-primary">
                {{ editingTask ? 'Save Changes' : 'Create Task' }}
              </button>
              <button type="button" (click)="closeModal()" class="btn-white">Cancel</button>
            </div>
          </form>

        </div>
      </div>
    }

    <!-- ═══ Delete Confirmation Modal ═══ -->
    @if (deleteModalOpen && deletingTask) {
      <div class="modal-backdrop" (click)="closeDeleteModal()"></div>
      <div class="modal-dialog" role="dialog">
        <div class="modal-panel">
          <div class="modal-header-row">
            <div class="modal-header-content">
              <h6 class="modal-pretitle">CONFIRM</h6>
              <h2 class="modal-title">Delete Task</h2>
              <p class="modal-subtitle">
                Are you sure you want to delete <strong>{{ deletingTask.title }}</strong>? This action cannot be undone.
              </p>
            </div>
            <button (click)="closeDeleteModal()" class="modal-close" aria-label="Close">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
                   fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          </div>
          <hr class="modal-divider">
          <div class="modal-actions">
            <button (click)="confirmDelete()" class="btn-danger">Delete</button>
            <button (click)="closeDeleteModal()" class="btn-white">Cancel</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    /* ---------- Header ---------- */
    .kanban-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 1.5rem;
    }
    .kanban-breadcrumb {
      font-size: 0.6875rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #95aac9;
    }
    .kanban-title {
      font-size: 1.5rem;
      font-weight: 700;
      color: #12263f;
      margin: 0;
    }
    .kanban-add-btn {
      display: inline-flex;
      align-items: center;
      gap: 0.375rem;
      background: #2c7be5;
      color: #fff;
      font-size: 0.875rem;
      font-weight: 500;
      padding: 0.5rem 1rem;
      border-radius: 0.375rem;
      text-decoration: none;
      transition: background 150ms;
    }
    .kanban-add-btn:hover { background: #1a68d1; }

    /* ---------- Board ---------- */
    .kanban-board {
      display: flex;
      gap: 1.25rem;
      overflow-x: auto;
      padding-bottom: 1rem;
      min-height: calc(100vh - 200px);
      align-items: flex-start;
    }

    /* ---------- Column ---------- */
    .kanban-column {
      flex: 0 0 340px;
      min-width: 340px;
      background: #fff;
      border: 1px solid #e3ebf6;
      border-radius: 0.5rem;
      display: flex;
      flex-direction: column;
    }
    .kanban-column-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1rem 1.25rem;
      border-bottom: 1px solid #e3ebf6;
    }
    .kanban-column-title {
      font-size: 1rem;
      font-weight: 600;
      color: #12263f;
      margin: 0;
    }
    .kanban-column-count {
      font-size: 0.75rem;
      font-weight: 600;
      color: #95aac9;
      background: #edf2f9;
      border-radius: 9999px;
      padding: 0.125rem 0.5rem;
    }

    /* ---------- Drop zone ---------- */
    .kanban-drop-zone {
      padding: 0.75rem;
      flex: 1;
      min-height: 100px;
    }
    .kanban-drop-zone.cdk-drop-list-dragging .kanban-card:not(.cdk-drag-placeholder) {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    /* ---------- Card ---------- */
    .kanban-card {
      background: #fff;
      border: 1px solid #e3ebf6;
      border-radius: 0.5rem;
      padding: 1rem 1.125rem;
      margin-bottom: 0.625rem;
      cursor: grab;
      transition: box-shadow 150ms, border-color 150ms;
    }
    .kanban-card:hover {
      border-color: #c6d3e6;
      box-shadow: 0 2px 8px rgba(18, 38, 63, 0.08);
    }
    .kanban-card.cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }
    .kanban-card-placeholder {
      background: transparent;
      border: 2px dashed #c6d3e6;
      border-radius: 0.5rem;
      min-height: 60px;
      margin-bottom: 0.625rem;
    }

    /* ---------- Card content ---------- */
    .kanban-card-badges {
      margin-bottom: 0.5rem;
    }
    .kanban-badge {
      font-size: 0.6875rem;
      font-weight: 600;
      padding: 0.125rem 0.5rem;
      border-radius: 0.25rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }
    .kanban-badge-pending   { background: #edf2f9; color: #95aac9; }
    .kanban-badge-progress  { background: #fff3e0; color: #e6a117; }
    .kanban-badge-completed { background: #e6f9ee; color: #00d97e; }

    .kanban-card-title {
      font-size: 0.9375rem;
      font-weight: 500;
      color: #12263f;
      margin: 0 0 0.25rem;
      line-height: 1.4;
    }
    .kanban-card-desc {
      font-size: 0.8125rem;
      color: #95aac9;
      margin: 0 0 0.625rem;
      line-height: 1.5;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    /* ---------- Card footer ---------- */
    .kanban-card-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: 0.625rem;
    }
    .kanban-card-meta {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .kanban-card-date {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.75rem;
      color: #95aac9;
    }
    .kanban-card-date svg { color: #b1c2d9; }

    .kanban-card-actions {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      opacity: 0;
      transition: opacity 150ms;
    }
    .kanban-card:hover .kanban-card-actions { opacity: 1; }
    .kanban-action-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 1.75rem;
      height: 1.75rem;
      border-radius: 0.25rem;
      border: none;
      background: transparent;
      color: #95aac9;
      cursor: pointer;
      transition: background 100ms, color 100ms;
      text-decoration: none;
    }
    .kanban-action-btn:hover { background: #edf2f9; color: #12263f; }
    .kanban-action-delete:hover { background: #fde8e8; color: #e63757; }

    /* ---------- Add Card link ---------- */
    .kanban-add-card {
      display: block;
      width: 100%;
      text-align: center;
      padding: 0.75rem;
      font-size: 0.8125rem;
      font-weight: 500;
      color: #95aac9;
      border: none;
      border-top: 1px solid #e3ebf6;
      background: transparent;
      cursor: pointer;
      text-decoration: none;
      transition: color 150ms, background 150ms;
    }
    .kanban-add-card:hover { color: #2c7be5; background: #f9fbfd; }

    /* ---------- Empty state ---------- */
    .kanban-empty {
      text-align: center;
      padding: 2rem 1rem;
      color: #b1c2d9;
      font-size: 0.8125rem;
    }

    /* ══════ Modal Overlay ══════ */
    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(18, 38, 63, 0.45);
      z-index: 1000;
      animation: fadeIn 150ms ease;
    }
    .modal-dialog {
      position: fixed;
      inset: 0;
      z-index: 1001;
      display: flex;
      align-items: flex-start;
      justify-content: center;
      padding: 3rem 1rem;
      overflow-y: auto;
      animation: slideIn 200ms ease;
    }
    .modal-panel {
      background: #fff;
      border: 1px solid #e3ebf6;
      border-radius: 0.5rem;
      box-shadow: 0 8px 32px rgba(18, 38, 63, 0.15);
      padding: 2rem;
      width: 100%;
      max-width: 600px;
    }

    /* Header */
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
      border: none;
      background: transparent;
      color: #95aac9;
      cursor: pointer;
      flex-shrink: 0;
      transition: background 100ms, color 100ms;
    }
    .modal-close:hover { background: #edf2f9; color: #12263f; }
    .modal-divider {
      border: none;
      border-top: 1px solid #e3ebf6;
      margin: 1.5rem 0;
    }

    /* Inner card */
    .inner-card {
      background: #fff;
      border: 1px solid #e3ebf6;
      border-radius: 0.5rem;
      margin-bottom: 1.25rem;
    }
    .inner-card-header {
      display: flex;
      align-items: center;
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

    /* Fields */
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

    /* Error */
    .form-error {
      background: #fde8e8;
      border: 1px solid #f5c6cb;
      color: #e63757;
      border-radius: 0.375rem;
      padding: 0.5rem 0.75rem;
      font-size: 0.8125rem;
      margin-bottom: 1.25rem;
    }

    /* Buttons */
    .modal-actions { display: flex; gap: 0.75rem; }
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
      transition: background 100ms, border-color 100ms;
    }
    .btn-white:hover { background: #f9fbfd; border-color: #c6d3e6; }
    .btn-danger {
      background: #e63757;
      color: #fff;
      border: none;
      padding: 0.5rem 1.25rem;
      border-radius: 0.375rem;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 150ms;
    }
    .btn-danger:hover { background: #c62a47; }

    /* Animations */
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
    @keyframes slideIn { from { opacity: 0; transform: translateY(-12px); } to { opacity: 1; transform: translateY(0); } }
  `],
})
export class TaskListComponent implements OnInit {
  columns: KanbanColumn[] = [
    { status: 'Pending', label: 'Pending', tasks: [] },
    { status: 'InProgress', label: 'In Progress', tasks: [] },
    { status: 'Completed', label: 'Completed', tasks: [] },
  ];

  /* ── Modal state ── */
  modalOpen = false;
  editingTask: TaskResponse | null = null;

  /* ── Delete modal state ── */
  deleteModalOpen = false;
  deletingTask: TaskResponse | null = null;
  formTitle = '';
  formDescription = '';
  formDueDate = '';
  formError: string | null = null;

  constructor(
    private api: ApiService,
    private auth: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.api.getTasks().subscribe(tasks => {
      for (const col of this.columns) {
        col.tasks = tasks.filter(t => t.status === col.status);
      }
    });
  }

  drop(event: CdkDragDrop<TaskResponse[]>, targetStatus: TaskItemStatus): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
      const task = event.container.data[event.currentIndex];
      task.status = targetStatus;
      this.api.changeTaskStatus(task.id, { status: targetStatus }).subscribe({
        next: () => this.toast.success(`Task moved to ${this.statusLabel(targetStatus)}`),
        error: (err) => { this.toast.error(this.extractError(err)); this.loadTasks(); },
      });
    }
  }

  deleteTask(task: TaskResponse): void {
    this.deletingTask = task;
    this.deleteModalOpen = true;
  }

  confirmDelete(): void {
    if (!this.deletingTask) return;
    const task = this.deletingTask;
    this.closeDeleteModal();
    this.api.deleteTask(task.id).subscribe({
      next: () => { this.toast.success('Task deleted'); this.loadTasks(); },
      error: (err) => this.toast.error(this.extractError(err)),
    });
  }

  closeDeleteModal(): void {
    this.deleteModalOpen = false;
    this.deletingTask = null;
  }

  /* ── Modal methods ── */
  openNewTask(): void {
    this.editingTask = null;
    this.formTitle = '';
    this.formDescription = '';
    this.formDueDate = '';
    this.formError = null;
    this.modalOpen = true;
  }

  openEditTask(task: TaskResponse): void {
    this.editingTask = task;
    this.formTitle = task.title;
    this.formDescription = task.description ?? '';
    this.formDueDate = task.dueDate ? task.dueDate.substring(0, 10) : '';
    this.formError = null;
    this.modalOpen = true;
  }

  closeModal(): void {
    this.modalOpen = false;
    this.editingTask = null;
  }

  saveModal(): void {
    this.formError = null;
    if (this.editingTask) {
      const req: UpdateTaskRequest = {
        title: this.formTitle,
        description: this.formDescription || undefined,
        dueDate: this.formDueDate ? this.formDueDate + 'T00:00:00.000Z' : undefined,
      };
      this.api.updateTask(this.editingTask.id, req).subscribe({
        next: () => { this.closeModal(); this.loadTasks(); this.toast.success('Task updated'); },
        error: err => (this.formError = this.extractError(err)),
      });
    } else {
      const req: CreateTaskRequest = {
        title: this.formTitle,
        description: this.formDescription || undefined,
        dueDate: this.formDueDate ? this.formDueDate + 'T00:00:00.000Z' : undefined,
      };
      this.api.createTask(req).subscribe({
        next: () => { this.closeModal(); this.loadTasks(); this.toast.success('Task created'); },
        error: err => (this.formError = this.extractError(err)),
      });
    }
  }

  badgeClass(status: TaskItemStatus): string {
    switch (status) {
      case 'Pending': return 'kanban-badge-pending';
      case 'InProgress': return 'kanban-badge-progress';
      case 'Completed': return 'kanban-badge-completed';
    }
  }

  statusLabel(status: TaskItemStatus): string {
    switch (status) {
      case 'Pending': return 'Pending';
      case 'InProgress': return 'In Progress';
      case 'Completed': return 'Completed';
    }
  }

  private extractError(err: any): string {
    const body = err?.error;
    if (typeof body === 'string' && body.length > 0) return body;
    if (typeof body === 'object' && body?.message) return body.message;
    if (err?.status === 403) return 'You do not have permission to perform this action.';
    if (err?.status === 404) return 'Task not found.';
    return err?.statusText || 'An unexpected error occurred.';
  }
}
