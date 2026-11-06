import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from '../auth/auth.store';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const token = authStore.token();

  const requisicaoAutenticada = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(requisicaoAutenticada).pipe(
    catchError((error: HttpErrorResponse) => {
      const ehRotaPublicaAuth =
        req.url.includes('/api/auth/login') || req.url.includes('/api/auth/registro');

      if (error.status === 401 && !ehRotaPublicaAuth) {
        authStore.sair();
      }

      return throwError(() => error);
    })
  );
};
