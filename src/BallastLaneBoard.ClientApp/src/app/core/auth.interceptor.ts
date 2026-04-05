import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  if (auth.isLoggedIn && req.url.startsWith('/api')) {
    const cloned = req.clone({
      setHeaders: { Authorization: `Bearer ${auth.accessToken}` },
    });
    return next(cloned);
  }

  return next(req);
};
