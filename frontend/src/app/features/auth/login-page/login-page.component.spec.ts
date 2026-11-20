import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { throwError } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { LoginPageComponent } from './login-page.component';

describe('LoginPageComponent', () => {
  let login: jasmine.Spy;
  let router: Router;

  beforeEach(async () => {
    login = jasmine.createSpy('login').and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 401 }))
    );

    await TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { login } },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({}) } }
        }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    spyOn(router, 'navigateByUrl').and.resolveTo(true);
  });

  it('muestra una alerta cuando las credenciales son invalidas', () => {
    const fixture = TestBed.createComponent(LoginPageComponent);
    fixture.detectChanges();
    fixture.componentInstance.formulario.setValue({
      email: 'usuario@ejemplo.com',
      password: 'incorrecta'
    });

    fixture.componentInstance.iniciarSesion();
    fixture.detectChanges();

    const alerta = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alerta.textContent).toContain('El correo o la contraseña no son válidos.');
    expect(login).toHaveBeenCalled();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('alterna la visibilidad de la contraseña', () => {
    const fixture = TestBed.createComponent(LoginPageComponent);
    fixture.detectChanges();

    const password = fixture.nativeElement.querySelector('#password') as HTMLInputElement;
    const toggle = fixture.nativeElement.querySelector(
      'button[aria-label="Mostrar contraseña"]'
    ) as HTMLButtonElement;
    expect(password.type).toBe('password');
    expect(password.classList).toContain('pr-11');

    toggle.click();
    fixture.detectChanges();

    expect(password.type).toBe('text');
    expect(fixture.nativeElement.querySelector('button[aria-label="Ocultar contraseña"]')).not.toBeNull();

    (fixture.nativeElement.querySelector('button[aria-label="Ocultar contraseña"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(password.type).toBe('password');
    expect(fixture.nativeElement.querySelector('button[aria-label="Mostrar contraseña"]')).not.toBeNull();
  });
});
