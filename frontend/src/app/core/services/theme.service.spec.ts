import { TestBed } from '@angular/core/testing';

import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.removeItem('bg_theme');
    document.documentElement.classList.remove('dark');
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    localStorage.removeItem('bg_theme');
    document.documentElement.classList.remove('dark');
  });

  it('aplica la preferencia guardada', () => {
    localStorage.setItem('bg_theme', 'dark');

    const service = TestBed.inject(ThemeService);

    expect(service.isDark()).toBeTrue();
    expect(document.documentElement.classList.contains('dark')).toBeTrue();
  });

  it('respeta la preferencia del sistema si no hay una guardada', () => {
    spyOn(window, 'matchMedia').and.returnValue({ matches: true } as MediaQueryList);

    const service = TestBed.inject(ThemeService);

    expect(service.isDark()).toBeTrue();
    expect(document.documentElement.classList.contains('dark')).toBeTrue();
  });

  it('persiste el cambio de tema y actualiza la clase del documento', () => {
    localStorage.setItem('bg_theme', 'light');
    const service = TestBed.inject(ThemeService);

    service.alternar();

    expect(service.isDark()).toBeTrue();
    expect(localStorage.getItem('bg_theme')).toBe('dark');
    expect(document.documentElement.classList.contains('dark')).toBeTrue();
  });
});
