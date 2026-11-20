import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { environment } from '../../../environments/environment';
import { ItemsResult } from '../models/items-result.models';
import { Proveedor } from '../models/proveedor.models';

@Injectable({ providedIn: 'root' })
export class ProveedorService {
  private readonly http = inject(HttpClient);

  listar() {
    return this.http.get<ItemsResult<Proveedor>>(`${environment.apiUrl}/proveedores`);
  }

  crear(nombre: string) {
    return this.http.post<Proveedor>(`${environment.apiUrl}/proveedores`, { nombre });
  }
}
