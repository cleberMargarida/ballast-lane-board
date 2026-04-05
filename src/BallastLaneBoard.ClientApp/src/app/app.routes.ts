import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'tasks', pathMatch: 'full' },
  {
    path: 'tasks',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./tasks/task-list.component').then(m => m.TaskListComponent),
  },
  {
    path: 'tasks/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./tasks/task-form.component').then(m => m.TaskFormComponent),
  },
  {
    path: 'tasks/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./tasks/task-form.component').then(m => m.TaskFormComponent),
  },
  {
    path: 'signup',
    loadComponent: () =>
      import('./auth/signup.component').then(m => m.SignupComponent),
  },
  {
    path: 'callback',
    loadComponent: () =>
      import('./auth/callback.component').then(m => m.CallbackComponent),
  },
  {
    path: '**',
    loadComponent: () =>
      import('./shared/not-found.component').then(m => m.NotFoundComponent),
  },
];
