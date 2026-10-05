import { HttpInterceptorFn } from '@angular/common/http';

const STORAGE_KEY = 'ekub_auth_session';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('/api')) {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const session = JSON.parse(stored);
        if (session?.token) {
          req = req.clone({
            setHeaders: {
              Authorization: `Bearer ${session.token}`
            }
          });
        }
      }
    } catch {
      // Fallback if localStorage is inaccessible
    }
  }

  return next(req);
};
