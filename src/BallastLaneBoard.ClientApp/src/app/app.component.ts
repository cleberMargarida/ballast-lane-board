import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar.component';
import { ToastComponent } from './shared/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, ToastComponent],
  template: `
    <app-navbar />
    <main class="kanban-main">
      <router-outlet />
    </main>
    <app-toast />
  `,
  styles: [`
    .kanban-main {
      max-width: 100%;
      padding: 1.5rem 2rem;
      min-height: calc(100vh - 56px);
    }
  `],
})
export class AppComponent {}
