import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiError } from '../models/api-error.models';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (request, next) => next(request).pipe(
  catchError((error: HttpErrorResponse) => {
    if (!request.url.startsWith(environment.apiUrl)) {
      return throwError(() => error);
    }

    if (error.status === 401) {
      inject(AuthService).logout();
    }

    const problem = error.error as ApiError | undefined;
    const detalles = problem?.errors
      ? Object.values(problem.errors).flat().join(' ')
      : undefined;
    const mensaje = detalles
      || problem?.detail
      || problem?.title
      || (error.status === 0 ? 'No se pudo conectar con el servidor.' : 'Ocurrio un error inesperado.');

    inject(NotificationService).mostrarError(mensaje);
    return throwError(() => error);
  })
);
