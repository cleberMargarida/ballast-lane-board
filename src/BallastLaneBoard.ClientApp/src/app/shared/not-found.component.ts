import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div style="text-align:center;padding:5rem 1rem;">
      <h1 style="font-size:3.5rem;font-weight:700;color:#95aac9;">404</h1>
      <p style="color:#6e84a3;margin-top:1rem;font-size:1.125rem;">Page not found</p>
      <a routerLink="/" style="color:#2c7be5;margin-top:1.5rem;display:inline-block;">Go Home</a>
    </div>
  `,
})
export class NotFoundComponent {}
