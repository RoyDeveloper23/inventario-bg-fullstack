import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, Usuario } from '../models/auth.models';

const SESION_KEY = 'inventario_sesion';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly sesion = signal<LoginResponse | null>(this.cargarSesion());

  readonly usuario = computed(() => this.sesion()?.usuario ?? null);
  readonly estaAutenticado = computed(() => {
    const sesion = this.sesion();
    return sesion !== null && new Date(sesion.expiresAtUtc).getTime() > Date.now();
  });

  readonly accessToken = computed(() => this.estaAutenticado() ? this.sesion()?.accessToken : null);

  login(datos: LoginRequest) {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, datos).pipe(
      tap((sesion) => {
        sessionStorage.setItem(SESION_KEY, JSON.stringify(sesion));
        this.sesion.set(sesion);
      })
    );
  }

  logout(redirigir = true): void {
    sessionStorage.removeItem(SESION_KEY);
    this.sesion.set(null);
    if (redirigir) {
      void this.router.navigate(['/login']);
    }
  }

  private cargarSesion(): LoginResponse | null {
    const valor = sessionStorage.getItem(SESION_KEY);
    if (!valor) {
      return null;
    }

    try {
      return JSON.parse(valor) as LoginResponse;
    } catch {
      sessionStorage.removeItem(SESION_KEY);
      return null;
    }
  }
}
