import { DOCUMENT } from '@angular/common';
import { inject, Injectable, signal } from '@angular/core';

const THEME_STORAGE_KEY = 'bg_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  readonly isDark = signal(false);

  constructor() {
    const window = this.document.defaultView;
    const preference = window?.localStorage.getItem(THEME_STORAGE_KEY);
    const isDark = preference === 'dark'
      || (preference !== 'light' && (window?.matchMedia('(prefers-color-scheme: dark)').matches ?? false));

    this.isDark.set(isDark);
    this.aplicarTema(isDark);
  }

  alternar(): void {
    const isDark = !this.isDark();
    this.isDark.set(isDark);
    this.aplicarTema(isDark);
    this.document.defaultView?.localStorage.setItem(THEME_STORAGE_KEY, isDark ? 'dark' : 'light');
  }

  private aplicarTema(isDark: boolean): void {
    this.document.documentElement.classList.toggle('dark', isDark);
  }
}
