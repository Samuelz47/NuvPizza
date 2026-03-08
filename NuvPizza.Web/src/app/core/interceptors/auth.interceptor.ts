import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // Se houver token, clona a requisição para adicionar o header Authorization
  const authReq = token
    ? req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        // Token expirado ou não autorizado — força logout e redireciona para login
        authService.logout();
      } else if (err.status === 429) {
        // Traduz e exibe o erro 429 Rate Limit
        const errorMessage = typeof err.error === 'string'
          ? err.error
          : 'Muitas requisições. Tente novamente mais tarde.';
        alert(errorMessage);
      }
      return throwError(() => err);
    })
  );
};