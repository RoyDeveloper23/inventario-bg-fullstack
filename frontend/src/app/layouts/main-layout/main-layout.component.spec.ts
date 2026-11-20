import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { MainLayoutComponent } from './main-layout.component';

describe('MainLayoutComponent', () => {
  let fixture: ComponentFixture<MainLayoutComponent>;

  beforeEach(() => {
    const isDark = signal(false);
    TestBed.configureTestingModule({
      imports: [MainLayoutComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: { usuario: () => ({ nombre: 'Roy' }), logout: jasmine.createSpy('logout') }
        },
        {
          provide: ThemeService,
          useValue: { isDark, alternar: () => isDark.update((value) => !value) }
        }
      ]
    });

    fixture = TestBed.createComponent(MainLayoutComponent);
  });

  it('renderiza iconos SVG independientes del texto del usuario y alterna el icono', () => {
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('.theme-toggle') as HTMLButtonElement;
    expect(fixture.nativeElement.querySelector('.user-name')?.textContent.trim()).toBe('Roy');
    expect(button.querySelector('span')).toBeNull();
    expect(button.querySelector('svg')).not.toBeNull();
    expect(button.textContent).not.toContain('☀');
    expect(button.textContent).not.toContain('☾');

    button.click();
    fixture.detectChanges();

    expect(button.getAttribute('aria-label')).toBe('Cambiar a modo claro');
    expect(button.querySelector('svg circle')).not.toBeNull();
  });
});
