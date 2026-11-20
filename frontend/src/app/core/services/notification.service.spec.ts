import { fakeAsync, TestBed, tick } from '@angular/core/testing';

import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  it('cierra una notificación a los 3500 ms', fakeAsync(() => {
    service.mostrarExito('Guardado');

    tick(3499);
    expect(service.notificacion()?.mensaje).toBe('Guardado');
    tick(1);
    expect(service.notificacion()).toBeNull();
  }));

  it('reinicia el autocierre si llega otra notificación', fakeAsync(() => {
    service.mostrarExito('Primera');
    tick(2000);
    service.mostrarError('Segunda');

    tick(2000);
    expect(service.notificacion()?.mensaje).toBe('Segunda');
    tick(1499);
    expect(service.notificacion()?.mensaje).toBe('Segunda');
    tick(1);
    expect(service.notificacion()).toBeNull();
  }));
});
