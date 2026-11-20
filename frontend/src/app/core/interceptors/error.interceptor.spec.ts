import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let client: HttpClient;
  let http: HttpTestingController;
  let logout: jasmine.Spy;
  let mostrarError: jasmine.Spy;

  beforeEach(() => {
    logout = jasmine.createSpy('logout');
    mostrarError = jasmine.createSpy('mostrarError');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { logout } },
        { provide: NotificationService, useValue: { mostrarError } }
      ]
    });

    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('no redirige ni muestra notificacion al fallar el login', () => {
    client.get(`${environment.apiUrl}/auth/login`).subscribe({
      error: () => undefined
    });

    http.expectOne(`${environment.apiUrl}/auth/login`).flush({}, {
      status: 401,
      statusText: 'Unauthorized'
    });

    expect(logout).not.toHaveBeenCalled();
    expect(mostrarError).not.toHaveBeenCalled();
  });

  it('cierra sesion y muestra el error en otros 401 del API', () => {
    client.get(`${environment.apiUrl}/productos`).subscribe({
      error: () => undefined
    });

    http.expectOne(`${environment.apiUrl}/productos`).flush({ title: 'Unauthorized' }, {
      status: 401,
      statusText: 'Unauthorized'
    });

    expect(logout).toHaveBeenCalled();
    expect(mostrarError).toHaveBeenCalledWith('Tu sesión ha expirado. Inicia sesión nuevamente.');
  });
});
