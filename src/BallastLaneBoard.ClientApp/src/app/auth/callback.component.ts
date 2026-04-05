import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-callback',
  standalone: true,
  template: `
    <div style="text-align:center;padding:4rem 1rem;color:#95aac9;">
      <div style="display:inline-block;width:24px;height:24px;border:3px solid #e3ebf6;border-top-color:#2c7be5;border-radius:50%;animation:spin .6s linear infinite;margin-bottom:1rem;"></div>
      <p style="font-size:.9375rem;">Signing in&hellip;</p>
    </div>
  `,
  styles: [`@keyframes spin { to { transform: rotate(360deg); } }`],
})
export class CallbackComponent {
  constructor(router: Router) {
    router.navigate(['/'], { replaceUrl: true });
  }
}
