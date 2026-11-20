import { Injectable, signal } from '@angular/core';

export interface Notification {
  mensaje: string;
  tipo: 'success' | 'error';
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  readonly notificacion = signal<Notification | null>(null);
  private timer: ReturnType<typeof setTimeout> | null = null;

  mostrarError(mensaje: string): void {
    this.notificacion.set({ mensaje, tipo: 'error' });
    this.programarCierre();
  }

  mostrarExito(mensaje: string): void {
    this.notificacion.set({ mensaje, tipo: 'success' });
    this.programarCierre();
  }

  cerrar(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
    this.notificacion.set(null);
  }

  private programarCierre(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
    }
    this.timer = setTimeout(() => {
      this.timer = null;
      this.notificacion.set(null);
    }, 3500);
  }
}
