import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiError } from '../models/api-error.models';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const notifications = inject(NotificationService);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (!request.url.startsWith(environment.apiUrl)) {
        return throwError(() => error);
      }

      if (request.url === `${environment.apiUrl}/auth/login`) {
        return throwError(() => error);
      }

      if (error.status === 401) {
        auth.logout();
      }

      const problem = error.error as ApiError | undefined;
      const detalles = problem?.errors
        ? Object.values(problem.errors).flat().join(' ')
        : undefined;
      const mensaje = error.status === 401
        ? 'Tu sesión ha expirado. Inicia sesión nuevamente.'
        : detalles
          || problem?.detail
          || problem?.title
          || (error.status === 0 ? 'No se pudo conectar con el servidor.' : 'Ocurrió un error inesperado.');

      notifications.mostrarError(mensaje);
      return throwError(() => error);
    })
  );
};
