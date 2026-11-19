import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { environment } from '../../../../environments/environment';
import { ItemsResult } from '../../../core/models/items-result.models';
import {
  ActualizarProductoInventarioRequest,
  CrearProductoInventarioRequest,
  ProductoInventario
} from '../models/inventario.models';

@Injectable({ providedIn: 'root' })
export class InventarioService {
  private readonly http = inject(HttpClient);

  private url(productoId: string): string {
    return `${environment.apiUrl}/productos/${productoId}/inventarios`;
  }

  listar(productoId: string) {
    return this.http.get<ItemsResult<ProductoInventario>>(this.url(productoId));
  }

  crear(productoId: string, datos: CrearProductoInventarioRequest) {
    return this.http.post<ProductoInventario>(this.url(productoId), datos);
  }

  actualizar(productoId: string, inventarioId: string, datos: ActualizarProductoInventarioRequest) {
    return this.http.put<ProductoInventario>(`${this.url(productoId)}/${inventarioId}`, datos);
  }

  eliminar(productoId: string, inventarioId: string) {
    return this.http.delete<void>(`${this.url(productoId)}/${inventarioId}`);
  }
}
