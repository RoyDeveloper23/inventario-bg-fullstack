import { Injectable, signal } from '@angular/core';

export interface Notification {
  mensaje: string;
  tipo: 'success' | 'error';
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  readonly notificacion = signal<Notification | null>(null);

  mostrarError(mensaje: string): void {
    this.notificacion.set({ mensaje, tipo: 'error' });
  }

  mostrarExito(mensaje: string): void {
    this.notificacion.set({ mensaje, tipo: 'success' });
  }

  cerrar(): void {
    this.notificacion.set(null);
  }
}
